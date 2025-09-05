/*
 * xoreos-tools - Tools to help with xoreos development
 *
 * This is a C# port of the original C++ code.
 *
 * xoreos-tools is the legal property of its developers, whose names
 * can be found in the AUTHORS file distributed with this source
 * distribution.
 *
 * xoreos-tools is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 *
 * xoreos-tools is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with xoreos-tools. If not, see <http://www.gnu.org/licenses/>.
 */

using AveTranslatorM.Fev;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace AveTranslatorM.Fev
{
    /// <summary>
    /// An FEV file is used to define events for the FMOD system
    /// and categorize them.
    /// There is currently only one relevant version of fev files
    /// with the FourCC "FEV1".
    /// </summary>
    public class FMODEventFile
    {
        #region Enums and Structs

        /// <summary>If an event is 2D or 3D.</summary>
        public enum EventMode { k2D, k3D }

        /// <summary>The Rollof 3D type.</summary>
        public enum Rollof3DType { kLogarithmic, kLinear, kCustom, kUnspecified }

        /// <summary>The type of the 3D position.</summary>
        public enum Position3DType { kWorldRelative, kHeadRelative }

        /// <summary>Possible types of streaming for FMOD.</summary>
        public enum StreamingType { kDecompressIntoMemory, kLoadIntoMemory, kStreamFromDisk }

        /// <summary>Possible Play modes.</summary>
        public enum PlayMode { kSequential = 0, kRandom, kRandomNoRepeat, kSequentialNoRepeat, kShuffle, kProgrammerSelected }

        /// <summary>Possible Property types.</summary>
        public enum PropertyType { kPropertyInt = 0, kPropertyFloat, kPropertyString }

        /// <summary>Some objects in FMOD can have generic properties.</summary>
        public struct Property
        {
            public PropertyType Type;
            // In C#, 'object' can hold different types, similar to boost::variant.
            public object Value;
        }

        /// <summary>Reference to an external wave bank.</summary>
        public struct WaveBank
        {
            public uint MaxStreams;
            public StreamingType StreamingType;
            public byte[] BeforeName;
            public string Name;
        }

        /// <summary>A category which is organized hierarchically.</summary>
        public struct Category
        {
            public string Name;
            public uint Volume;
            public uint Pitch;
        }

        /// <summary>An event category for storing events.</summary>
        public struct EventCategory
        {
            public string Name;
        }

        public struct EventLayer
        {
            public short Priority;
        }


        /// <summary>
        /// Представляє вбудований параметр події FMOD,
        /// такий як Distance, Direction або Elevation.
        /// </summary>
        public class EventParameter
        {
            /// <summary>
            /// Числовий ідентифікатор вбудованого параметра.
            /// Наприклад, в FMOD Studio "Distance" зазвичай має ID 0.
            /// </summary>
            public uint BuiltInParameterId { get; set; }

            /// <summary>
            /// Мінімальне значення діапазону параметра, яке відстежується.
            /// </summary>
            public float RangeMin { get; set; }

            /// <summary>
            /// Максимальне значення діапазону параметра, яке відстежується.
            /// </summary>
            public float RangeMax { get; set; }

            public EventParameter()
            {
                // Встановлюємо значення за замовчуванням
                BuiltInParameterId = 0;
                RangeMin = 0.0f;
                RangeMax = 1.0f;
            }
        }

        /// <summary>
        /// An FMOD event.
        /// Note: Most of the floating point values only represent a
        /// range from 0.0f to 1.0f, which are mapped to different
        /// decibel values.
        /// </summary>
        public struct Event
        {
            public uint TypeId;
            public string Name;
            public Guid Guid;
            public float Volume;
            public float Pitch;
            public float PitchRandomization;
            public float VolumeRandomization;
            public uint Priority;
            public EventMode Mode;
            public uint MaxPlaybacks;
            public uint MaxPlaybacksBehavior;
            public Rollof3DType Rollof3D;
            public Position3DType Position3D;
            public uint PositionRandomization3D;
            public float ConeInsideAngle3D;
            public float ConeOutsideAngle3D;
            public float ConeOutsideVolume3D; // [0, 1] -> [0, -1024]
            public float DopplerFactor3D;
            public float SpeakerSpread3D;
            public float PanLevel3D;
            public float MinDistance3D;
            public float MaxDistance3D;
            public float Speaker2DL;
            public float Speaker2DR;
            public float Speaker2DC;
            public float SpeakerLFE;
            public float Speaker2DLR;
            public float Speaker2DRR;
            public float Speaker2DLS;
            public float Speaker2DRS;
            public float ReverbDryLevel;
            public float ReverbWetLevel;
            public uint FadeInTime;
            public uint FadeOutTime;
            public float SpawnIntensity;
            public float SpawnIntensityRandomization;
            public List<EventParameter> Parameters;
            public Dictionary<string, Property> UserProperties;
            public string Category;
            public List<EventLayer> Layers;
        }

        /// <summary>A sound definition.</summary>
        public struct SoundDefinition
        {
            public PlayMode PlayMode;
            public string Name;
            public uint SpawnTimeMin;
            public uint SpawnTimeMax;
            public uint MaximumSpawnedSounds;
            public float Volume;
            public float VolumeRandomization;
            public float Pitch;
            public float PitchRandomization;
            public float Position3DRandomization;
        }

        /// <summary>A reverb definition.</summary>
        public struct ReverbDefinition
        {
            public string Name;
            public int Room;
            public int RoomHF;
            public float RoomRollof;
            public float DecayTime;
            public float DecayHFRatio;
            public int Reflections;
            public float ReflectDelay;
            public int Reverb;
            public float ReverbDelay;
            public float Diffusion;
            public float Density;
            public float HfReference;
            public int RoomLF;
            public float LfReference;
        }

        #endregion

        private static readonly uint kFEVID = (uint)'F' << 24 | (uint)'E' << 16 | (uint)'V' << 8 | '1';

        public string BankName { get; private set; }
        public List<WaveBank> WaveBanks { get; private set; }
        public List<Category> Categories { get; private set; }
        public List<Event> Events { get; private set; }
        public List<SoundDefinition> Definitions { get; private set; }
        public List<ReverbDefinition> Reverbs { get; private set; }

        public FMODEventFile(string resRef)
        {
            using (var stream = new FileStream(resRef, FileMode.Open, FileAccess.Read))
            {
                Load(stream);
            }
        }

        public FMODEventFile(Stream fevStream)
        {
            Load(fevStream);
        }

        private void Load(Stream fevStream)
        {
            using (var reader = new BinaryReader(fevStream, Encoding.ASCII, true))
            {
                // In C#, we need to manually reverse the byte order for Big Endian values.
                var magicBytes = reader.ReadBytes(4);
                Array.Reverse(magicBytes);
                uint magic = BitConverter.ToUInt32(magicBytes, 0);

                if (magic != kFEVID)
                    throw new Exception("FMODEventFile.Load(): Invalid magic number");

                reader.BaseStream.Seek(280, SeekOrigin.Current); // Skip unknown values
                BankName = ReadLengthPrefixedString(reader);

                uint numWaveBanks = reader.ReadUInt32();
                WaveBanks = new List<WaveBank>((int)numWaveBanks);
                for (uint i = 0; i < numWaveBanks; ++i)
                {
                    var wb = new WaveBank();
                    uint streamingType = reader.ReadUInt32();
                    wb.MaxStreams = reader.ReadUInt32();
                    wb.BeforeName = reader.ReadBytes(8);
                    switch (streamingType)
                    {
                        case 0x00010000: wb.StreamingType = StreamingType.kDecompressIntoMemory; break;
                        case 0x00020000: wb.StreamingType = StreamingType.kLoadIntoMemory; break;
                        case 0x0B000000: wb.StreamingType = StreamingType.kStreamFromDisk; break;
                        default: wb.StreamingType = StreamingType.kDecompressIntoMemory; break; // Default case
                    }

                    wb.Name = ReadLengthPrefixedString(reader);
                    WaveBanks.Add(wb);
                }

                Categories = new List<Category>();
                ReadCategory(reader);

                Events = new List<Event>();
                uint numEventGroups = reader.ReadUInt32();
                for (uint i = 0; i < numEventGroups; ++i)
                {
                    ReadEventCategory(reader);
                }

                uint numSoundDefinitionTemplates = reader.ReadUInt32();
                var definitionTemplates = new List<SoundDefinition>((int)numSoundDefinitionTemplates);
                for (uint i = 0; i < numSoundDefinitionTemplates; ++i)
                {
                    var def = new SoundDefinition
                    {
                        PlayMode = (PlayMode)reader.ReadUInt32(),
                        SpawnTimeMin = reader.ReadUInt32(),
                        SpawnTimeMax = reader.ReadUInt32(),
                        MaximumSpawnedSounds = reader.ReadUInt32(),
                        Volume = ReadSingleLE(reader)
                    };
                    reader.BaseStream.Seek(12, SeekOrigin.Current);
                    def.VolumeRandomization = ReadSingleLE(reader);
                    def.Pitch = ReadSingleLE(reader);
                    reader.BaseStream.Seek(12, SeekOrigin.Current);
                    def.PitchRandomization = ReadSingleLE(reader);
                    def.Position3DRandomization = ReadSingleLE(reader);
                    definitionTemplates.Add(def);
                }

                uint numSoundDefinitions = reader.ReadUInt32();
                Definitions = new List<SoundDefinition>((int)numSoundDefinitions);
                for (uint i = 0; i < numSoundDefinitions; ++i)
                {
                    string name = ReadLengthPrefixedString(reader);
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    uint templateId = reader.ReadUInt32();

                    SoundDefinition definition = definitionTemplates[(int)templateId];
                    definition.Name = name;
                    Definitions.Add(definition);
                }

                uint numReverbDefinitions = reader.ReadUInt32();
                Reverbs = new List<ReverbDefinition>((int)numReverbDefinitions);
                for (uint i = 0; i < numReverbDefinitions; ++i)
                {
                    var reverb = new ReverbDefinition
                    {
                        Name = ReadLengthPrefixedString(reader),
                        Room = reader.ReadInt32(),
                        RoomHF = reader.ReadInt32(),
                        RoomRollof = ReadSingleLE(reader),
                        DecayTime = ReadSingleLE(reader),
                        DecayHFRatio = ReadSingleLE(reader),
                        Reflections = reader.ReadInt32(),
                        ReflectDelay = ReadSingleLE(reader),
                        Reverb = reader.ReadInt32(),
                        ReverbDelay = ReadSingleLE(reader),
                        Diffusion = ReadSingleLE(reader),
                        Density = ReadSingleLE(reader),
                        HfReference = ReadSingleLE(reader),
                        RoomLF = reader.ReadInt32(),
                        LfReference = ReadSingleLE(reader)
                    };
                    reader.BaseStream.Seek(76, SeekOrigin.Current);
                    Reverbs.Add(reverb);
                }
            }
        }

        private void ReadCategory(BinaryReader reader)
        {
            var category = new Category
            {
                Name = ReadLengthPrefixedString(reader),
                Volume = (uint)ReadSingleLE(reader),
                Pitch = (uint)ReadSingleLE(reader)
            };
            reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip unknown

            Categories.Add(category);

            uint numSubCategories = reader.ReadUInt32();
            for (uint i = 0; i < numSubCategories; ++i)
            {
                ReadCategory(reader);
            }
        }

        int dEc = 0;
        int dE = 0;
        private void ReadEventCategory(BinaryReader reader)
        {
            string name = ReadLengthPrefixedString(reader);
             ReadProperties(reader); // Properties are read but not stored for the category itself
            uint numSubEventCategories = reader.ReadUInt32();
            uint numEvents = reader.ReadUInt32();

            for (uint i = 0; i < numSubEventCategories; ++i)
            {
                Debug.WriteLine($"deC: {dEc++}");
                ReadEventCategory(reader);
            }

            for (uint i = 0; i < numEvents; ++i)
            {
                Debug.WriteLine($"de: {dE++}");
                ReadEvent(reader);
            }
        }

        private void ReadEvent(BinaryReader reader)
        {
            var evt = new Event();
            evt.TypeId = reader.ReadUInt32();
            evt.Name = ReadLengthPrefixedString(reader);
            evt.Guid = new Guid(reader.ReadBytes(16));

            evt.Volume = ReadSingleLE(reader); // 1
            evt.Pitch = ReadSingleLE(reader); // 0
            evt.PitchRandomization = ReadSingleLE(reader); // 0
            evt.VolumeRandomization = ReadSingleLE(reader); // 0
            evt.Priority = reader.ReadUInt32(); // 64 (40hex)
            evt.MaxPlaybacks = reader.ReadUInt32(); // 1

            reader.BaseStream.Seek(4, SeekOrigin.Current);
            // unknown 10000
            var modeBytes = reader.ReadBytes(4); //mode!
            Array.Reverse(modeBytes);
            uint mode = BitConverter.ToUInt32(modeBytes, 0);

            if ((mode & 0x10000000) != 0) evt.Mode = EventMode.k3D;
            else if ((mode & 0x08000000) != 0) evt.Mode = EventMode.k2D;
            else throw new Exception("Invalid event mode");

            if ((mode & 0x00001000) != 0) evt.Rollof3D = Rollof3DType.kLogarithmic;
            else if ((mode & 0x00002000) != 0) evt.Rollof3D = Rollof3DType.kLinear;
            else if ((mode & 0x00000004) != 0) evt.Rollof3D = Rollof3DType.kCustom;
            else evt.Rollof3D = Rollof3DType.kUnspecified;

            if ((mode & 0x00000400) != 0) evt.Position3D = Position3DType.kHeadRelative;
            else evt.Position3D = Position3DType.kWorldRelative;

            evt.MinDistance3D = ReadSingleLE(reader);
            evt.MaxDistance3D = ReadSingleLE(reader);
            evt.Speaker2DL = ReadSingleLE(reader);
            evt.Speaker2DR = ReadSingleLE(reader);
            evt.Speaker2DC = ReadSingleLE(reader);
            evt.SpeakerLFE = ReadSingleLE(reader);
            evt.Speaker2DLR = ReadSingleLE(reader);
            evt.Speaker2DRR = ReadSingleLE(reader);
            evt.Speaker2DLS = ReadSingleLE(reader);
            evt.Speaker2DRS = ReadSingleLE(reader);
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            evt.ConeInsideAngle3D = ReadSingleLE(reader);
            evt.ConeOutsideAngle3D = ReadSingleLE(reader);
            evt.ConeOutsideVolume3D = ReadSingleLE(reader); // 1.0
            evt.MaxPlaybacksBehavior = reader.ReadUInt32(); // 1
            evt.DopplerFactor3D = ReadSingleLE(reader); // 1.0
            evt.ReverbDryLevel = ReadSingleLE(reader); // 0
            evt.ReverbWetLevel = ReadSingleLE(reader); // 0
            evt.SpeakerSpread3D = ReadSingleLE(reader);
            evt.FadeInTime = reader.ReadUInt32();
            evt.FadeOutTime = reader.ReadUInt32();
            evt.SpawnIntensity = ReadSingleLE(reader);
            evt.SpawnIntensityRandomization = ReadSingleLE(reader);
            evt.PanLevel3D = ReadSingleLE(reader);
            evt.PositionRandomization3D = reader.ReadUInt32();

            uint numLayers = reader.ReadUInt32();
            evt.Layers = new List<EventLayer>((int)numLayers);
            for (uint i = 0; i < numLayers; ++i)
            {
                var layer = new EventLayer();
                reader.BaseStream.Seek(2, SeekOrigin.Current);
                layer.Priority = reader.ReadInt16();
                reader.BaseStream.Seek(6, SeekOrigin.Current);
                evt.Layers.Add(layer);
            }

         
            reader.BaseStream.Seek(evt.TypeId == 16 ? 36 : 4, SeekOrigin.Current); // 4 ?
            evt.UserProperties = ReadProperties(reader);
            reader.BaseStream.Seek(evt.TypeId == 16 ? 12 : 4, SeekOrigin.Current); // 4 ?
            evt.Category = ReadLengthPrefixedString(reader);

            Events.Add(evt);
        }

        private Dictionary<string, Property> ReadProperties(BinaryReader reader)
        {
            uint numUserProperties = reader.ReadUInt32();
            var properties = new Dictionary<string, Property>((int)numUserProperties);
            for (uint i = 0; i < numUserProperties; ++i)
            {
                string propertyName = ReadLengthPrefixedString(reader);
                var property = new Property
                {
                    Type = (PropertyType)reader.ReadUInt32()
                };

                switch (property.Type)
                {
                    case PropertyType.kPropertyInt:
                        property.Value = reader.ReadInt32();
                        break;
                    case PropertyType.kPropertyFloat:
                        property.Value = ReadSingleLE(reader);
                        break;
                    case PropertyType.kPropertyString:
                        property.Value = ReadLengthPrefixedString(reader);
                        break;
                    default:
                        throw new Exception($"Invalid property type {property.Type}");
                }
                properties[propertyName] = property;
            }
            return properties;
        }

        private string ReadLengthPrefixedString(BinaryReader reader)
        {
            uint length = reader.ReadUInt32();
            if (length == 0) return string.Empty;
            byte[] bytes = reader.ReadBytes((int)length);
            return Encoding.ASCII.GetString(bytes).TrimEnd('\0');
        }

        private float ReadSingleLE(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToSingle(bytes, 0);
        }
    }
    /// <summary>
    /// Dumps the contents of an FMODEventFile into an XML file.
    /// </summary>
    public static class FEVDumper
    {
        public static void Dump(Stream output, Stream input)
        {
            var fev = new Fev.FMODEventFile(input);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                Encoding = new UTF8Encoding(false) // UTF-8 without BOM
            };

            using (var xml = XmlWriter.Create(output, settings))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("fev");
                xml.WriteAttributeString("bankname", fev.BankName);

                DumpWavebanks(xml, fev.WaveBanks);
                DumpEvents(xml, fev.Events);
                DumpReverbs(xml, fev.Reverbs);

                xml.WriteEndElement(); // fev
                xml.WriteEndDocument();
            }
        }

        private static void DumpWavebanks(XmlWriter xml, IEnumerable<FMODEventFile.WaveBank> waveBanks)
        {
            xml.WriteStartElement("wavebanks");
            foreach (var waveBank in waveBanks)
            {
                xml.WriteStartElement("wavebank");

                xml.WriteElementString("name", waveBank.Name);
                xml.WriteElementString("maxstreams", waveBank.MaxStreams.ToString());

                string bankType = waveBank.StreamingType switch
                {
                    FMODEventFile.StreamingType.kDecompressIntoMemory => "DecompressIntoMemory",
                    FMODEventFile.StreamingType.kStreamFromDisk => "StreamFromDisk",
                    FMODEventFile.StreamingType.kLoadIntoMemory => "LoadIntoMemory",
                    _ => "Unknown"
                };
                xml.WriteElementString("banktype", bankType);

                xml.WriteEndElement(); // wavebank
            }
            xml.WriteEndElement(); // wavebanks
        }

        private static void DumpEvents(XmlWriter xml, IEnumerable<FMODEventFile.Event> events)
        {
            xml.WriteStartElement("events");
            foreach (var evt in events)
            {
                xml.WriteStartElement("event");

                xml.WriteElementString("name", evt.Name);

                string mode = evt.Mode == FMODEventFile.EventMode.k2D ? "2D" : "3D";
                xml.WriteElementString("mode", mode);

                xml.WriteElementString("category", evt.Category);

                xml.WriteStartElement("volume");
                xml.WriteAttributeString("randomization", evt.VolumeRandomization.ToString("F6"));
                xml.WriteString(evt.Volume.ToString("F6"));
                xml.WriteEndElement();

                xml.WriteStartElement("pitch");
                xml.WriteAttributeString("randomization", evt.PitchRandomization.ToString("F6"));
                xml.WriteString(evt.Pitch.ToString("F6"));
                xml.WriteEndElement();

                xml.WriteElementString("priority", evt.Priority.ToString());
                xml.WriteElementString("nmaxplaybacks", evt.MaxPlaybacks.ToString());
                xml.WriteElementString("maxplaybacksbehaviour", evt.MaxPlaybacksBehavior.ToString());

                string rolloff = evt.Rollof3D switch
                {
                    FMODEventFile.Rollof3DType.kLogarithmic => "Logarithmic",
                    FMODEventFile.Rollof3DType.kLinear => "Linear",
                    FMODEventFile.Rollof3DType.kCustom => "Custom",
                    FMODEventFile.Rollof3DType.kUnspecified => "Unspecified",
                    _ => "Unknown"
                };
                xml.WriteElementString("rolloff3d", rolloff);

                xml.WriteElementString("mindistance3d", evt.MinDistance3D.ToString("F6"));
                xml.WriteElementString("maxdistance3d", evt.MaxDistance3D.ToString("F6"));

                xml.WriteStartElement("position3d");
                xml.WriteAttributeString("randomization", evt.PositionRandomization3D.ToString());
                string position = evt.Position3D == FMODEventFile.Position3DType.kWorldRelative ? "WorldRelative" : "HeadRelative";
                xml.WriteString(position);
                xml.WriteEndElement();

                xml.WriteElementString("coneinsideangle3d", evt.ConeInsideAngle3D.ToString("F6"));
                xml.WriteElementString("coneoutsideangle3d", evt.ConeOutsideAngle3D.ToString("F6"));
                xml.WriteElementString("outsidevolume3d", evt.ConeOutsideVolume3D.ToString("F6"));
                xml.WriteElementString("dopplerfactor3d", evt.DopplerFactor3D.ToString("F6"));
                xml.WriteElementString("speakerspread3d", evt.SpeakerSpread3D.ToString("F6"));
                xml.WriteElementString("panlevel3d", evt.PanLevel3D.ToString("F6"));

                xml.WriteElementString("speakerl2d", evt.Speaker2DL.ToString("F6"));
                xml.WriteElementString("speakerc2d", evt.Speaker2DC.ToString("F6"));
                xml.WriteElementString("speakerr2d", evt.Speaker2DR.ToString("F6"));
                xml.WriteElementString("speakerlr2d", evt.Speaker2DLR.ToString("F6"));
                xml.WriteElementString("speakerrr2d", evt.Speaker2DRR.ToString("F6"));
                xml.WriteElementString("speakerls2d", evt.Speaker2DLS.ToString("F6"));
                xml.WriteElementString("speakerrs2d", evt.Speaker2DRS.ToString("F6"));
                xml.WriteElementString("speakerlfe", evt.SpeakerLFE.ToString("F6"));

                xml.WriteElementString("reverbdrylevel", evt.ReverbDryLevel.ToString("F6"));
                xml.WriteElementString("reverbwetlevel", evt.ReverbWetLevel.ToString("F6"));

                xml.WriteElementString("fadeintime", evt.FadeInTime.ToString());
                xml.WriteElementString("fadeouttime", evt.FadeOutTime.ToString());

                xml.WriteStartElement("spawnintensity");
                xml.WriteAttributeString("randomization", evt.SpawnIntensityRandomization.ToString("F6"));
                xml.WriteString(evt.SpawnIntensity.ToString("F6"));
                xml.WriteEndElement();

                DumpUserProperties(xml, evt.UserProperties);

                xml.WriteEndElement(); // event
            }
            xml.WriteEndElement(); // events
        }

        private static void DumpReverbs(XmlWriter xml, IEnumerable<FMODEventFile.ReverbDefinition> reverbs)
        {
            xml.WriteStartElement("reverbs"); // Corrected from "reverb" to "reverbs" for plurality
            foreach (var reverb in reverbs)
            {
                xml.WriteStartElement("reverb"); // A single reverb definition
                xml.WriteElementString("name", reverb.Name);
                xml.WriteElementString("room", reverb.Room.ToString());
                xml.WriteElementString("roomhf", reverb.RoomHF.ToString());
                xml.WriteElementString("decaytime", reverb.DecayTime.ToString("F6"));
                xml.WriteElementString("decayhfratio", reverb.DecayHFRatio.ToString("F6"));
                xml.WriteElementString("reflections", reverb.Reflections.ToString());
                xml.WriteElementString("reflectdelay", reverb.ReflectDelay.ToString("F6"));
                xml.WriteElementString("reverb", reverb.Reverb.ToString());
                xml.WriteElementString("reverbdelay", reverb.ReverbDelay.ToString("F6"));
                xml.WriteElementString("hfreference", reverb.HfReference.ToString("F6"));
                xml.WriteElementString("roomlf", reverb.RoomLF.ToString());
                xml.WriteElementString("lfreference", reverb.LfReference.ToString("F6"));
                xml.WriteEndElement(); // reverb
            }
            xml.WriteEndElement(); // reverbs
        }

        private static void DumpUserProperties(XmlWriter xml, IReadOnlyDictionary<string, FMODEventFile.Property> properties)
        {
            xml.WriteStartElement("userproperties");
            foreach (var prop in properties)
            {
                xml.WriteStartElement("property");
                xml.WriteAttributeString("name", prop.Key);

                string typeStr = prop.Value.Type switch
                {
                    FMODEventFile.PropertyType.kPropertyInt => "int",
                    FMODEventFile.PropertyType.kPropertyFloat => "float",
                    FMODEventFile.PropertyType.kPropertyString => "string",
                    _ => "unknown"
                };
                xml.WriteAttributeString("type", typeStr);

                string valueStr = prop.Value.Type switch
                {
                    FMODEventFile.PropertyType.kPropertyFloat => ((float)prop.Value.Value).ToString("F6"),
                    _ => prop.Value.Value.ToString()
                };
                xml.WriteString(valueStr);

                xml.WriteEndElement(); // property
            }
            xml.WriteEndElement(); // userproperties
        }
    }
}

 /*         private void ReadEvent(BinaryReader reader)
        {
            var evt = new Event();
            evt.TypeId = reader.ReadUInt32();
            evt.Name = ReadLengthPrefixedString(reader);
            evt.Guid = new Guid(reader.ReadBytes(16));

            evt.Volume = ReadSingleLE(reader); // 1
            evt.Pitch = ReadSingleLE(reader); // 0
            evt.PitchRandomization = ReadSingleLE(reader); // 0
            evt.VolumeRandomization = ReadSingleLE(reader); // 0
            evt.Priority = reader.ReadUInt32(); // 64 (40hex)
            evt.MaxPlaybacks = reader.ReadUInt32(); // 1
            
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            // unknown 10000
            var modeBytes = reader.ReadBytes(4); //mode!
            Array.Reverse(modeBytes);
            uint mode = BitConverter.ToUInt32(modeBytes, 0);  

            if ((mode & 0x10000000) != 0) evt.Mode = EventMode.k3D;
            else if ((mode & 0x08000000) != 0) evt.Mode = EventMode.k2D;
            else throw new Exception("Invalid event mode");

            if ((mode & 0x00001000) != 0) evt.Rollof3D = Rollof3DType.kLogarithmic;
            else if ((mode & 0x00002000) != 0) evt.Rollof3D = Rollof3DType.kLinear;
            else if ((mode & 0x00000004) != 0) evt.Rollof3D = Rollof3DType.kCustom;
            else evt.Rollof3D = Rollof3DType.kUnspecified;

            if ((mode & 0x00000400) != 0) evt.Position3D = Position3DType.kHeadRelative;
            else evt.Position3D = Position3DType.kWorldRelative;

            evt.MinDistance3D = ReadSingleLE(reader);
            evt.MaxDistance3D = ReadSingleLE(reader);
            evt.Speaker2DL = ReadSingleLE(reader);
            evt.Speaker2DR = ReadSingleLE(reader);
            evt.Speaker2DC = ReadSingleLE(reader);
            evt.SpeakerLFE = ReadSingleLE(reader);
            evt.Speaker2DLR = ReadSingleLE(reader);
            evt.Speaker2DRR = ReadSingleLE(reader);
            evt.Speaker2DLS = ReadSingleLE(reader);
            evt.Speaker2DRS = ReadSingleLE(reader);
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            evt.ConeInsideAngle3D = ReadSingleLE(reader);
            evt.ConeOutsideAngle3D = ReadSingleLE(reader);
            evt.ConeOutsideVolume3D = ReadSingleLE(reader); // 1.0
            evt.MaxPlaybacksBehavior = reader.ReadUInt32(); // 1
            evt.DopplerFactor3D = ReadSingleLE(reader); // 1.0
            evt.ReverbDryLevel = ReadSingleLE(reader); // 0
            evt.ReverbWetLevel = ReadSingleLE(reader); // 0
            evt.SpeakerSpread3D = ReadSingleLE(reader);
            evt.FadeInTime = reader.ReadUInt32();
            evt.FadeOutTime = reader.ReadUInt32();
            evt.SpawnIntensity = ReadSingleLE(reader);
            evt.SpawnIntensityRandomization = ReadSingleLE(reader);
            evt.PanLevel3D = ReadSingleLE(reader);
            evt.PositionRandomization3D = reader.ReadUInt32();

            uint numLayers = reader.ReadUInt32();
            evt.Layers = new List<EventLayer>((int)numLayers);
            for (uint i = 0; i < numLayers; ++i)
            {
                var layer = new EventLayer();
                reader.BaseStream.Seek(2, SeekOrigin.Current);
                layer.Priority = reader.ReadInt16();
                reader.BaseStream.Seek(6, SeekOrigin.Current);
                evt.Layers.Add(layer);
            }

            reader.BaseStream.Seek(36, SeekOrigin.Current); // 4 ?
            evt.UserProperties = ReadProperties(reader);
            reader.BaseStream.Seek(12, SeekOrigin.Current); // Always 1? // 4?
            evt.Category = ReadLengthPrefixedString(reader);

            Events.Add(evt);
        }

*/