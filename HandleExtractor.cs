using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BG3LocalizationMerger
{
    internal static partial class HandleExtractor
    {
        [GeneratedRegex("""<attribute[ \w="]* handle="(\w+)"[ \w="]*/>""")]
        private static partial Regex BookHandle();

        [GeneratedRegex("""<attribute(?:[\s\w="]*\s(?:id="Type"|value="item")){2}\s*/>""")]
        private static partial Regex TemplateItemFilter();

        [GeneratedRegex("""<attribute(?:[\s\w="]*\s(?:id="Type"|value="character")){2}\s*/>""")]
        private static partial Regex TemplateCharacterFilter();

        [GeneratedRegex(
            """<attribute(?:(?:[\s\w="]*\s(?:id="DisplayName"|handle="(?<Name>\w{30,})")){2}|(?:[\s\w="]*\s(?:id="Description"|handle="(?<Description>\w{30,})")){2})[\s\w="]*/>"""
        )]
        private static partial Regex TemplateHandles();

        [GeneratedRegex(
            """<attribute(?:[\s\w="]*\s(?:id="(?:DisplayName|Title)"|handle="(\w{30,})")){2}[\s\w="]*/>"""
        )]
        private static partial Regex CharacterHandles();

        [GeneratedRegex("""<\w+ .*=['"](\w{37})["'].*/>""")]
        private static partial Regex GenericHandle();

        [GeneratedRegex("""<\w+ handle=['"](\w{37})["'].*/>""")]
        private static partial Regex HandleHandle();

        [GeneratedRegex(
            """data "(?<Type>(?:Extra)?Description|DisplayName)" "(?<Handle>\w{37});\d+"\s?"""
        )]
        private static partial Regex StatusHandle();

        [GeneratedRegex(
            """<attribute id="(?<Type>\w+)" type="TranslatedString" handle="(?<Handle>\w{37})".*/>"""
        )]
        private static partial Regex QuestHandle();

        [GeneratedRegex(
            """(?:<(?:TextBlock x:Name|Setter TargetName)="Description")\s?""",
            RegexOptions.IgnoreCase
        )]
        private static partial Regex TooltipFilter();

        public static IEnumerable<string> ExtractCharacterHandles(string path)
        {
            var text = ReadText(path);
            if (!TemplateCharacterFilter().IsMatch(text))
                return Enumerable.Empty<string>();
            return ExtractHandles(text, CharacterHandles());
        }

        public static IEnumerable<string> ExtractWaypointHandles(string path)
        {
            return ExtractGenericHandles(path);
        }

        public static IEnumerable<string> ExtractQuestMarkerHandles(string path)
        {
            return ExtractGenericHandles(path);
        }

        public static IEnumerable<string> ExtractBookHandles(string path)
        {
            return ExtractGenericHandles(path);
        }

        public static (IEnumerable<string>, IEnumerable<string>) ExtractQuestHandles(string path)
        {
            string text = ReadText(path);
            MatchCollection matches = QuestHandle().Matches(text);
            var group = matches.GroupBy(x => x.Groups["Type"].Value);
            return (
                group
                    .FirstOrDefault(x => x.Key == "QuestTitle")
                    ?.SelectMany(x => x.Groups.Values.Skip(1))
                    .Select(x => x.Value) ?? Enumerable.Empty<string>(),
                group
                    .Where(x => x.Key.EndsWith("Description"))
                    .SelectMany(x => x)
                    ?.SelectMany(x => x.Groups.Values.Skip(1))
                    .Select(x => x.Value) ?? Enumerable.Empty<string>()
            );
        }

        public static (IEnumerable<string>, IEnumerable<string>) ExtractStatusHandles(string path)
        {
            string text = ReadText(path);
            MatchCollection matches = StatusHandle().Matches(text);
            var group = matches.GroupBy(x => x.Groups["Type"].Value);
            return (
                group
                    .FirstOrDefault(x => x.Key == "DisplayName")
                    ?.SelectMany(x => x.Groups.Values.Skip(1))
                    .Select(x => x.Value) ?? Enumerable.Empty<string>(),
                group
                    .Where(x => x.Key.EndsWith("Description"))
                    .SelectMany(x => x)
                    ?.SelectMany(x => x.Groups.Values.Skip(1))
                    .Select(x => x.Value) ?? Enumerable.Empty<string>()
            );
        }

        public static IEnumerable<Group> ExtractItemGroup(string path)
        {
            string text = ReadText(path);
            if (!TemplateItemFilter().IsMatch(text))
                return Enumerable.Empty<Group>();
            MatchCollection matches = TemplateHandles().Matches(text);
            return matches
                .SelectMany(x => x.Groups.Values.Skip(1))
                .Where(x => !string.IsNullOrEmpty(x.Value));
        }

        public static IEnumerable<string> ExtractTooltipHandles(string path)
        {
            string text = ReadText(path);
            MatchCollection matches = GenericHandle().Matches(text);
            return matches
                .Where(x => TooltipFilter().IsMatch(x.Groups[0].Value))
                .SelectMany(x => x.Groups.Values.Skip(1))
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrEmpty(x));
        }

        public static IEnumerable<string> ExtractHintHandles(string path)
        {
            return ExtractGenericHandles(path);
        }

        public static IEnumerable<string> ExtractGenericHandles(string path)
        {
            return ExtractHandles(ReadText(path), GenericHandle());
        }

        private static string ReadText(string path)
        {
            using var sr = new StreamReader(path);
            return sr.ReadToEnd();
        }

        private static IEnumerable<string> ExtractHandles(string text, Regex regex)
        {
            MatchCollection matches = regex.Matches(text);
            return matches
                .SelectMany(x => x.Groups.Values.Skip(1))
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrEmpty(x));
        }

        private static IEnumerable<Group> ExtractGroups(string text, Regex regex)
        {
            MatchCollection matches = regex.Matches(text);
            return matches
                .SelectMany(x => x.Groups.Values.Skip(1))
                .Where(x => !string.IsNullOrEmpty(x.Value));
        }
    }
}
