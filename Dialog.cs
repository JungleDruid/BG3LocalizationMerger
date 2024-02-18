using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace BG3LocalizationMerger
{
    public enum DialogType
    {
        Unknown,
        TagGreeting,
        TagQuestion,
        TagAnswer,
        Jump,
        ActiveRoll,
        RollResult,
        NestedDialog,
        VisualState,
        TagCinematic,
        Alias,
        Pop,
        FallibleQuestionResult,
        Trade,
        PassiveRoll,
    }

    internal class Dialog
    {
        public string Id { get; }
        public DialogType DialogType { get; }
        public string? Speaker { get; }
        public IEnumerable<string> Children { get; }
        public IEnumerable<string> Texts { get; }

        public Dialog(XmlReader reader)
        {
            ArrayPool<string> shared = ArrayPool<string>.Shared;
            string[] children = shared.Rent(128);
            int childrenCount = 0;
            string[] texts = shared.Rent(128);
            int textsCount = 0;
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                switch (reader.Name)
                {
                    case "attribute":
                        switch (reader.GetAttribute("id")!)
                        {
                            case "constructor":
                                if (
                                    Enum.TryParse(
                                        reader.GetAttribute("value")?.Replace(" ", ""),
                                        out DialogType type
                                    )
                                )
                                {
                                    DialogType = type;
                                }
                                else
                                {
                                    Console.WriteLine(
                                        $"Unknown dialog constructor: {reader.GetAttribute("value")}"
                                    );
                                }
                                break;
                            case "UUID":
                                Id = reader.GetAttribute("value")!;
                                break;
                        }
                        break;
                    case "children":
                        Debug.Assert(!reader.IsEmptyElement);
                        var subreader = reader.ReadSubtree();
                        subreader.ReadToDescendant("node");
                        do
                        {
                            if (
                                subreader.NodeType != XmlNodeType.Element
                                || subreader.IsEmptyElement
                            )
                            {
                                subreader.Read();
                                continue;
                            }
                            switch (subreader.GetAttribute("id"))
                            {
                                case "children":
                                    ParseChildren(
                                        subreader.ReadSubtree(),
                                        children,
                                        ref childrenCount
                                    );
                                    break;
                                case "TaggedTexts":
                                    ParseTexts(subreader.ReadSubtree(), texts, ref textsCount);
                                    break;
                                default:
                                    reader.Skip();
                                    break;
                            }
                        } while (
                            subreader.NodeType == XmlNodeType.Element && !subreader.IsEmptyElement
                            || subreader.Read()
                        );
                        break;
                    default:
                        break;
                }
            }

            Children = childrenCount == 0 ? Array.Empty<string>() : children[..childrenCount];
            Texts = textsCount == 0 ? Array.Empty<string>() : texts[..textsCount];
            Debug.Assert(!string.IsNullOrWhiteSpace(Id));
            shared.Return(children);
            shared.Return(texts);
        }

        private static void ParseTexts(XmlReader reader, string[] texts, ref int textsCount)
        {
            if (!reader.ReadToDescendant("children"))
                throw new InvalidOperationException();

            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
                {
                    continue;
                }

                Debug.Assert(reader.GetAttribute("id") == "TaggedText");

                var textReader = reader.ReadSubtree();
                if (!textReader.Read() || !textReader.ReadToDescendant("node"))
                    throw new InvalidOperationException();
                while (
                    textReader.NodeType == XmlNodeType.Element && !textReader.IsEmptyElement
                    || textReader.Read()
                )
                {
                    if (textReader.IsEmptyElement || textReader.NodeType != XmlNodeType.Element)
                        continue;
                    switch (textReader.GetAttribute("id"))
                    {
                        case "TagTexts":
                            if (
                                !textReader.ReadToDescendant("children")
                                || !textReader.ReadToDescendant("attribute")
                            )
                                throw new InvalidOperationException();
                            do
                            {
                                if (textReader.NodeType != XmlNodeType.Element)
                                    continue;
                                if (
                                    textReader.Name == "attribute"
                                    && textReader.GetAttribute("id") == "TagText"
                                )
                                {
                                    texts[textsCount++] = textReader.GetAttribute("handle")!;
                                }
                            } while (textReader.Read());
                            break;
                        default:
                            textReader.Skip();
                            break;
                    }
                }
            }
        }

        private static void ParseChildren(
            XmlReader reader,
            string[] children,
            ref int childrenCount
        )
        {
            if (!reader.ReadToDescendant("children"))
                throw new InvalidOperationException();

            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
                {
                    continue;
                }
                Debug.Assert(reader.Name == "node" && reader.GetAttribute("id") == "child");
                reader.Read();
                Debug.Assert(
                    reader.NodeType == XmlNodeType.Element
                        && reader.Name == "attribute"
                        && reader.GetAttribute("id") == "UUID"
                        && reader.GetAttribute("value") != null
                );
                children[childrenCount++] = reader.GetAttribute("value")!;
            }
        }
    }
}
