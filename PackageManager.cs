using BG3LocalizationMerger.Resources.Strings;
using LSLib.LS;
using LSLib.LS.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BG3LocalizationMerger
{
    internal sealed partial class PackageManager : SingletonBase<PackageManager>
    {
        internal static readonly ImmutableList<string> s_modNames = new List<string>()
        {
            "Shared",
            "SharedDev",
            "Gustav",
            "GustavDev"
        }.ToImmutableList();

        private static readonly string[] s_questPrototypeFiles = new string[]
        {
            "quest_prototypes.lsx",
            "objective_prototypes.lsx"
        };

        public IReadOnlyDictionary<string, Dialog>? Dialogs { get; private set; }
        public ISet<string>? ItemNameHandles { get; private set; }
        public ISet<string>? ItemDescriptionHandles { get; private set; }
        public ISet<string>? CharacterHandles { get; private set; }
        public ISet<string>? BookHandles { get; private set; }
        public ISet<string>? TooltipHandles { get; private set; }
        public ISet<string>? HintsHandles { get; private set; }
        public ISet<string>? StatusNameHandles { get; private set; }
        public ISet<string>? StatusDescriptionHandles { get; private set; }
        public ISet<string>? QuestNameHandles { get; private set; }
        public ISet<string>? QuestDescriptionHandles { get; private set; }
        public ISet<string>? QuestMarkerHandles { get; private set; }
        public ISet<string>? WaypointHandles { get; private set; }
        public ISet<string>? MiscHandles { get; private set; }

        [GeneratedRegex(@"^(Shared|Gustav|Patch.+)$", RegexOptions.Compiled)]
        private static partial Regex ModDirRegex();

        public void Load(CancellationToken cancellationToken)
        {
            LoadItems(cancellationToken);
            LoadCharacters(cancellationToken);
            LoadDialogues(cancellationToken);
            LoadBooks(cancellationToken);
            LoadTooltips(cancellationToken);
            LoadHints(cancellationToken);
            LoadStatus(cancellationToken);
            LoadQuests(cancellationToken);
            LoadQuestMarkers(cancellationToken);
            LoadWaypoints(cancellationToken);
            LoadMiscs(cancellationToken);
        }

        private static void LogLoading(string text)
        {
            MainWindow.Log(string.Format(Strings.LoadingMessage, text));
        }

        private static void LogLoaded(int amount, string text, TimeSpan elapsed)
        {
            MainWindow.Log(
                string.Format(Strings.LoadedMessage, amount, text, elapsed.TotalSeconds)
            );
        }

        private void LoadCharacters(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (!props.MergeCharacters || CharacterHandles != null)
                return;

            LogLoading(Strings.Characters);
            Stopwatch sw = Stopwatch.StartNew();
            var modDirs = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Public"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "RootTemplates"))
                .Where(Directory.Exists);

            var files = modDirs.SelectMany(x => Directory.EnumerateFiles(x, "*.lsx"));

            var mergedFiles = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Mods"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Globals"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Select(x => Path.Combine(x, "Characters", "_merged.lsf.lsx"))
                .Where(File.Exists);

            var levelMergedFiles = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Mods"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Levels"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Select(x => Path.Combine(x, "Characters", "_merged.lsf.lsx"))
                .Where(File.Exists);

            CharacterHandles = files
                .Concat(mergedFiles)
                .Concat(levelMergedFiles)
                .AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(HandleExtractor.ExtractCharacterHandles)
                .ToHashSet();

            sw.Stop();
            LogLoaded(CharacterHandles.Count, Strings.Characters, sw.Elapsed);
        }

        private void LoadMiscs(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (!props.MergeMiscs || MiscHandles != null)
                return;

            LogLoading(Strings.Miscs);
            Stopwatch sw = Stopwatch.StartNew();

            var modDirs = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .SelectMany(Directory.GetDirectories)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Localization"))
                .Where(Directory.Exists);

            var files = modDirs.SelectMany(x => Directory.EnumerateFiles(x, "*Misc*.lsx"));

            MiscHandles = files
                .AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(HandleExtractor.ExtractGenericHandles)
                .ToHashSet();

            sw.Stop();
            LogLoaded(MiscHandles!.Count, Strings.Miscs, sw.Elapsed);
        }

        private void LoadWaypoints(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (!props.MergeWaypoints || WaypointHandles != null)
                return;

            LogLoading(Strings.Waypoints);
            Stopwatch sw = Stopwatch.StartNew();

            var modDirs = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Mods"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Localization"))
                .Where(Directory.Exists);

            var files = modDirs.SelectMany(
                x => Directory.EnumerateFiles(x, "Waypointshrines*.lsx")
            );

            WaypointHandles = files
                .AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(HandleExtractor.ExtractWaypointHandles)
                .ToHashSet();

            sw.Stop();
            LogLoaded(WaypointHandles!.Count, Strings.Waypoints, sw.Elapsed);
        }

        private void LoadQuestMarkers(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (!props.MergeQuestMarkers || QuestMarkerHandles != null)
                return;

            LogLoading(Strings.QuestMarkers);
            Stopwatch sw = Stopwatch.StartNew();

            var modDirs = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Mods"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Story", "Journal", "Markers"))
                .Where(Directory.Exists);

            var files = modDirs.SelectMany(x => Directory.EnumerateFiles(x, "*.lsx"));

            QuestMarkerHandles = files
                .AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(HandleExtractor.ExtractQuestMarkerHandles)
                .ToHashSet();

            sw.Stop();
            LogLoaded(QuestMarkerHandles!.Count, Strings.QuestMarkers, sw.Elapsed);
        }

        private void LoadQuests(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (
                (!props.MergeQuestNames || QuestNameHandles != null)
                && (!props.MergeQuestDescriptions || QuestDescriptionHandles != null)
            )
                return;

            LogLoading(Strings.Quests);
            Stopwatch sw = Stopwatch.StartNew();

            var modDirs = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Mods"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Story", "Journal"))
                .Where(Directory.Exists);

            var files = modDirs
                .SelectMany(x => Directory.EnumerateFiles(x, "*.lsx"))
                .Where(x => s_questPrototypeFiles.Contains(Path.GetFileName(x)));

            var handles = files
                .AsParallel()
                .WithCancellation(cancellationToken)
                .Select(HandleExtractor.ExtractQuestHandles);

            QuestNameHandles = handles.SelectMany(x => x.Item1).ToHashSet();
            QuestDescriptionHandles = handles.SelectMany(x => x.Item2).ToHashSet();

            sw.Stop();
            LogLoaded(
                QuestNameHandles!.Count + QuestDescriptionHandles!.Count,
                Strings.Quests,
                sw.Elapsed
            );
        }

        private void LoadStatus(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (
                (!props.MergeStatusNames || StatusNameHandles != null)
                && (!props.MergeStatusDescriptions || StatusDescriptionHandles != null)
            )
                return;

            LogLoading(Strings.Status);
            Stopwatch sw = Stopwatch.StartNew();

            var modDirs = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Public"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Stats", "Generated", "Data"))
                .Where(Directory.Exists);

            var files = modDirs.SelectMany(x => Directory.EnumerateFiles(x, "*.txt"));

            var handles = files
                .AsParallel()
                .WithCancellation(cancellationToken)
                .Select(HandleExtractor.ExtractStatusHandles);

            StatusNameHandles = handles.SelectMany(x => x.Item1).ToHashSet();
            StatusDescriptionHandles = handles.SelectMany(x => x.Item2).ToHashSet();

            sw.Stop();
            LogLoaded(
                StatusNameHandles!.Count + StatusDescriptionHandles!.Count,
                Strings.Status,
                sw.Elapsed
            );
        }

        private void LoadHints(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (!props.MergeHints || HintsHandles != null)
                return;

            LogLoading(Strings.Hints);
            Stopwatch sw = Stopwatch.StartNew();

            var files = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Select(x => Path.Combine(x, "Public", "Game", "Hints", "Hints.lsx"))
                .Where(Path.Exists)
                .Order();

            HintsHandles = files
                .AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(HandleExtractor.ExtractHintHandles)
                .ToHashSet();

            sw.Stop();
            LogLoaded(HintsHandles!.Count, Strings.Hints, sw.Elapsed);
        }

        private void LoadTooltips(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (!props.MergeTooltips || TooltipHandles != null)
                return;

            LogLoading(Strings.Tooltips);
            Stopwatch sw = Stopwatch.StartNew();

            var files = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Select(x => Path.Combine(x, "Public", "Game", "GUI", "Library", "Tooltips.xaml"))
                .Where(Path.Exists)
                .Order();

            TooltipHandles = files
                .AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(HandleExtractor.ExtractTooltipHandles)
                .ToHashSet();

            sw.Stop();
            LogLoaded(TooltipHandles!.Count, Strings.Tooltips, sw.Elapsed);
        }

        private void LoadBooks(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (!props.MergeBooks || BookHandles != null)
                return;

            LogLoading(Strings.Books);
            Stopwatch sw = Stopwatch.StartNew();

            var modDirs = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .SelectMany(Directory.GetDirectories)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Localization"))
                .Where(Directory.Exists);

            var files = modDirs.SelectMany(x => Directory.EnumerateFiles(x, "*Book*.lsx"));

            BookHandles = files
                .AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(HandleExtractor.ExtractBookHandles)
                .ToHashSet();

            sw.Stop();
            LogLoaded(BookHandles!.Count, Strings.Books, sw.Elapsed);
        }

        private void LoadItems(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (
                (!props.MergeItemNames || ItemNameHandles != null)
                && (!props.MergeItemDescriptions || ItemDescriptionHandles != null)
            )
                return;

            LogLoading(Strings.Items);
            Stopwatch sw = Stopwatch.StartNew();
            var modDirs = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Public"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "RootTemplates"))
                .Where(Directory.Exists);

            var files = modDirs.SelectMany(x => Directory.EnumerateFiles(x, "*.lsx"));

            var mergedFiles = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Mods"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Globals"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Select(x => Path.Combine(x, "Items", "_merged.lsf.lsx"))
                .Where(File.Exists);

            var levelMergedFiles = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Mods"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Levels"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Select(x => Path.Combine(x, "Items", "_merged.lsf.lsx"))
                .Where(File.Exists);

            var groups = files
                .Concat(mergedFiles)
                .Concat(levelMergedFiles)
                .AsParallel()
                .WithCancellation(cancellationToken)
                .Select(HandleExtractor.ExtractItemGroup);

            ItemNameHandles = groups
                .SelectMany(x => x.Where(x => x.Name == "Name").Select(x => x.Value))
                .ToHashSet();
            ItemDescriptionHandles = groups
                .SelectMany(x => x.Where(x => x.Name == "Description").Select(x => x.Value))
                .ToHashSet();

            sw.Stop();
            LogLoaded(
                ItemNameHandles?.Count ?? ItemDescriptionHandles!.Count,
                Strings.Items,
                sw.Elapsed
            );
        }

        private void LoadDialogues(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;
            if (!props.MergeDialogues || Dialogs != null)
                return;

            LogLoading(Strings.Dialogues);
            Stopwatch sw = Stopwatch.StartNew();
            var modDirs = Directory
                .GetDirectories(props.UnpackedDataPath)
                .Where(x => ModDirRegex().IsMatch(Path.GetFileName(x)))
                .Select(x => Path.Combine(x, "Mods"))
                .Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(x => s_modNames.Contains(Path.GetFileName(x)))
                .Order(new ModPriorityComparer())
                .Select(x => Path.Combine(x, "Story", "DialogsBinary"))
                .Where(Directory.Exists);

            Dialogs = modDirs
                .SelectMany(
                    dir => Directory.EnumerateFiles(dir, "*.lsx", SearchOption.AllDirectories)
                )
                .AsParallel()
                .WithCancellation(cancellationToken)
                .Select(lsx =>
                {
                    var speakers = new List<string?>(1);
                    XmlReader reader = XmlReader.Create(
                        lsx,
                        new XmlReaderSettings { IgnoreWhitespace = true }
                    );
                    if (!reader.ReadToFollowing("save") || !reader.ReadToDescendant("region"))
                        throw new InvalidOperationException();
                    while (reader.GetAttribute("id") != "dialog")
                        reader.Skip();
                    if (!reader.ReadToDescendant("children"))
                        throw new InvalidOperationException();
                    reader = reader.ReadSubtree();
                    if (!reader.ReadToDescendant("node"))
                        throw new InvalidOperationException();
                    do
                    {
                        if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
                        {
                            reader.Read();
                            continue;
                        }
                        Debug.Assert(reader.Name == "node");
                        string? id = reader.GetAttribute("id");
                        switch (id)
                        {
                            case "speakerlist":
                                ParseSpeakers(reader.ReadSubtree(), speakers);
                                break;
                            case "nodes":
                                var dialogs = ParseDialogs(reader.ReadSubtree(), speakers);
                                return dialogs;
                            default:
                                reader.Skip();
                                break;
                        }
                    } while (reader.NodeType == XmlNodeType.Element || reader.Read());
                    throw new InvalidOperationException();
                })
                .SelectMany(x => x)
                .GroupBy(x => x.Id)
                .Select(x => x.Last())
                .ToDictionary(x => x.Id);

            sw.Stop();
            LogLoaded(Dialogs.Count, Strings.Dialogs, sw.Elapsed);
        }

        public IEnumerable<string>? DialogHandles =>
            Dialogs?.Values
                .Where(x => x.Texts.Any())
                .SelectMany(x => x.Texts)
                .Where(x => x != null);

        private IEnumerable<Dialog> ParseDialogs(XmlReader reader, IList<string?> speakers)
        {
            if (reader.ReadToDescendant("children") && reader.ReadToDescendant("node"))
            {
                do
                {
                    if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
                    {
                        reader.Read();
                        continue;
                    }
                    if (reader.GetAttribute("id") != "node")
                    {
                        reader.Skip();
                        continue;
                    }
                    yield return new Dialog(reader.ReadSubtree(), speakers);
                } while (reader.NodeType == XmlNodeType.Element || reader.Read());
            }
        }

        private static void ParseSpeakers(XmlReader reader, IList<string?> speakers)
        {
            if (reader.ReadToDescendant("children") && reader.ReadToDescendant("node"))
            {
                do
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;
                    Debug.Assert(
                        reader.Name == "node"
                            && reader.GetAttribute("id") == "speaker"
                            && !reader.IsEmptyElement
                    );
                    var subreader = reader.ReadSubtree();
                    subreader.Read();
                    int index = -1;
                    string? id = null;
                    while (subreader.Read())
                    {
                        if (subreader.NodeType != XmlNodeType.Element)
                            continue;
                        switch (reader.GetAttribute("id"))
                        {
                            case "index":
                                index = int.Parse(reader.GetAttribute("value")!);
                                break;
                            case "list":
                                id = reader.GetAttribute("value");
                                break;
                        }
                    }
                    Debug.Assert(speakers.Count == index);
                    speakers.Add(id);
                    Debug.Assert(reader.NodeType == XmlNodeType.EndElement);
                } while (reader.Read());
            }
        }

        private class Locas
        {
            public XDocument Doc { get; }
            public XDocument RefDoc { get; }

            private readonly string _xmlPath;
            private readonly string _locaPath;
            private readonly string _dir;

            public Locas(CancellationToken cancellationToken)
            {
                var props = Properties.Settings.Default;

                string basePath = Path.Combine(Path.GetTempPath(), "BG3DialogMerger");
                _dir = Path.Combine(basePath, "LanguagePack");
                string refPackDir = Path.Combine(basePath, "ReferencePack");

                if (Path.Exists(basePath))
                {
                    Directory.Delete(basePath, true);
                }

                MainWindow.Log(string.Format(Strings.ExtractingMessage, Strings.LanguagePack));
                string langPackPath = props.LanguagePackPath;
                string refPackPath = props.ReferencePackPath;
                Packager packager = new();
                packager.UncompressPackage(langPackPath, _dir);
                packager.UncompressPackage(refPackPath, refPackDir);
                cancellationToken.ThrowIfCancellationRequested();

                _locaPath = Directory
                    .EnumerateFiles(_dir, "*.loca", SearchOption.AllDirectories)
                    .First();
                string refLocaPath = Directory
                    .EnumerateFiles(refPackDir, "*.loca", SearchOption.AllDirectories)
                    .First();
                _xmlPath = Path.Combine(
                    Path.GetDirectoryName(_locaPath)!,
                    Path.GetFileNameWithoutExtension(_locaPath) + ".xml"
                );
                string refXmlPath = Path.Combine(
                    Path.GetDirectoryName(refLocaPath)!,
                    Path.GetFileNameWithoutExtension(refLocaPath) + ".xml"
                );
                cancellationToken.ThrowIfCancellationRequested();

                MainWindow.Log(string.Format(Strings.ConvertingMessage, "LOCA", "XML"));
                LocaUtils.Save(
                    LocaUtils.Load(_locaPath, LocaFormat.Loca),
                    _xmlPath,
                    LocaFormat.Xml
                );
                LocaUtils.Save(
                    LocaUtils.Load(refLocaPath, LocaFormat.Loca),
                    refXmlPath,
                    LocaFormat.Xml
                );
                cancellationToken.ThrowIfCancellationRequested();

                MainWindow.Log(string.Format(Strings.LoadingMessage, "XML"));
                Doc = XDocument.Load(_xmlPath);
                RefDoc = XDocument.Load(refXmlPath);
            }

            public async Task Save(CancellationToken cancellationToken)
            {
                MainWindow.Log(string.Format(Strings.SavingMessage, "XML"));
                Doc.Save(_xmlPath);
                LocaUtils.Save(LocaUtils.Load(_xmlPath), _locaPath);
                File.Delete(_xmlPath);
                cancellationToken.ThrowIfCancellationRequested();
                MainWindow.Log(string.Format(Strings.PackagingMessage));
                string exportPath = Properties.Settings.Default.ExportPath;
                Game game = Game.BaldursGate3;
                var options = new PackageCreationOptions
                {
                    Version = game.PAKVersion(),
                    Compression = CompressionMethod.Zlib
                };

                Packager packager = new();
                while (true)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        packager.CreatePackage(exportPath, _dir, options);
                        break;
                    }
                    catch (IOException)
                    {
                        if (
                            !await Utils.ShowYesNoMessage(
                                MainWindow.Instance,
                                Strings.PackageErrorMessage
                            )
                        )
                        {
                            break;
                        }
                    }
                }
                MainWindow.Log(Strings.ExportCompleteMessage);
            }
        }

        public async Task Merge(CancellationToken cancellationToken)
        {
            var props = Properties.Settings.Default;

            Locas locas = new(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            Load(cancellationToken);

            MainWindow.Log(Strings.SortingDataMessage);
            var brHandles = Enumerable.Empty<string>();
            var br2Handles = Enumerable.Empty<string>();
            var separatorHandles = Enumerable.Empty<string>();
            var lineHandles = Enumerable.Empty<string>();

            static void MergeHandles(
                bool available,
                Func<IEnumerable<string>?> target,
                ref IEnumerable<string> parent
            )
            {
                if (available)
                    parent = parent.Concat(target.Invoke()!);
            }
            // csharpier-ignore-start
            MergeHandles(props.MergeDialogues, () => DialogHandles, ref brHandles);
            MergeHandles(props.MergeItemNames, () => ItemNameHandles, ref lineHandles);
            MergeHandles(props.MergeItemDescriptions, () => ItemDescriptionHandles, ref separatorHandles);
            MergeHandles(props.MergeCharacters, () => CharacterHandles, ref lineHandles);
            MergeHandles(props.MergeBooks, () => BookHandles, ref br2Handles);
            MergeHandles(props.MergeTooltips, () => TooltipHandles, ref brHandles);
            MergeHandles(props.MergeHints, () => HintsHandles, ref brHandles);
            MergeHandles(props.MergeStatusNames, () => StatusNameHandles, ref lineHandles);
            MergeHandles(props.MergeStatusDescriptions, () => StatusDescriptionHandles, ref separatorHandles);
            MergeHandles(props.MergeQuestNames, () => QuestNameHandles, ref lineHandles);
            MergeHandles(props.MergeQuestDescriptions, () => QuestDescriptionHandles, ref lineHandles);
            MergeHandles(props.MergeQuestMarkers, () => QuestMarkerHandles, ref lineHandles);
            MergeHandles(props.MergeWaypoints, () => WaypointHandles, ref brHandles);
            MergeHandles(props.MergeMiscs, () => MiscHandles, ref lineHandles);
            // csharpier-ignore-end

            var brSet = brHandles.ToHashSet();
            var br2Set = br2Handles.ToHashSet();
            var lineSet = lineHandles.ToHashSet();

            var combined = brSet
                .Concat(br2Set)
                .Concat(separatorHandles)
                .Concat(lineSet)
                .ToHashSet();

            var refDict = locas.RefDoc
                .Element("contentList")!
                .Elements("content")
                .Select(x => (x.Attribute("contentuid")!.Value, x.Value))
                .Where(x => combined.Contains(x.Item1))
                .ToDictionary(x => x.Item1, x => x.Item2);
            var dict = locas.Doc
                .Element("contentList")!
                .Elements("content")
                .Select(x => (x, x.Attribute("contentuid")!.Value))
                .Where(x => refDict.ContainsKey(x.Value))
                .ToDictionary(x => x.Value, x => x.x);

            MainWindow.Log(string.Format(Strings.TotalStringMessage, combined.Count, dict.Count));

            MainWindow.Log(string.Format(Strings.MergingMessage, "XML"));
            string Separator = new('-', 40);
            ParallelOptions parallelOptions = new() { CancellationToken = cancellationToken };
            Parallel.ForEach(
                dict,
                parallelOptions,
                item =>
                {
                    var refValue = refDict[item.Key];
                    XElement element = item.Value;
                    string value = element.Value.Trim();
                    if (brSet.Contains(item.Key))
                    {
                        if (value.EndsWith("<br>", StringComparison.OrdinalIgnoreCase))
                        {
                            element.Value = value + refValue;
                        }
                        else
                        {
                            element.Value = $"{value}<br>{refValue}";
                        }
                    }
                    else if (br2Set.Contains(item.Key))
                    {
                        if (value.EndsWith("<br>", StringComparison.OrdinalIgnoreCase))
                        {
                            element.Value = $"{value}<br>{refValue}";
                        }
                        else
                        {
                            element.Value = $"{value}<br><br>{refValue}";
                        }
                    }
                    else if (separatorHandles.Contains(item.Key))
                    {
                        if (value.EndsWith("<br>", StringComparison.OrdinalIgnoreCase))
                        {
                            element.Value = $"{value}{Separator}<br>{refValue}";
                        }
                        else
                        {
                            element.Value = $"{value}<br>{Separator}<br>{refValue}";
                        }
                    }
                    else if (lineSet.Contains(item.Key))
                    {
                        element.Value = $"{value} [{refValue}]";
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                    var version = int.Parse(element.Attribute("version")!.Value);
                    element.SetAttributeValue("version", version + 1);
                }
            );

            await locas.Save(cancellationToken);
        }

        public async Task MergeUnconditionally(CancellationToken cancellationToken)
        {
            Locas locas = new(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            MainWindow.Log(Strings.MergingUnconditionallyMessage);
            var refDict = locas.RefDoc
                .Element("contentList")!
                .Elements("content")
                .Select(x => (x.Attribute("contentuid")!.Value, x.Value))
                .ToDictionary(x => x.Item1, x => x.Item2);
            cancellationToken.ThrowIfCancellationRequested();
            var dict = locas.Doc
                .Element("contentList")!
                .Elements("content")
                .Select(x => (x, x.Attribute("contentuid")!.Value))
                .Where(x => refDict.ContainsKey(x.Value))
                .ToDictionary(x => x.Value, x => x.x);
            cancellationToken.ThrowIfCancellationRequested();

            Parallel.ForEach(
                dict,
                kv =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    kv.Value.Value += $" [{refDict[kv.Key]}]";
                }
            );

            await locas.Save(cancellationToken);
        }
    }

    public sealed class ModPriorityComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            Debug.Assert(x != null && y != null);
            string xDirName = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(x)))!;
            string yDirName = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(y)))!;
            int xDirOrder = GetOrder(xDirName);
            int yDirOrder = GetOrder(yDirName);
            if (xDirOrder != yDirOrder)
                return xDirOrder.CompareTo(yDirOrder);
            int dirCompare = xDirName.CompareTo(yDirName);
            if (dirCompare != 0)
                return dirCompare;
            string xModName = Path.GetFileName(x);
            string yModName = Path.GetFileName(y);
            return GetOrder(xModName).CompareTo(GetOrder(yModName));
        }

        private static int GetOrder(string dir)
        {
            var index = PackageManager.s_modNames.IndexOf(dir);
            if (index < 0)
                return PackageManager.s_modNames.Count;
            return index;
        }
    }
}
