/*
 * moFileReader - A simple .mo-File-Reader
 * Copyright (C) 2009 Domenico Gentner (scorcher24@gmail.com)
 * Copyright (C) 2018-2021 Edgar (Edgar@AnotherFoxGuy.com)
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 *   1. Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *
 *   2. Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *
 *   3. The names of its contributors may not be used to endorse or promote
 *      products derived from this software without specific prior written
 *      permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/** \namespace moFileLib
 * \brief This is the only namespace of this small sourcecode.
 */
namespace MoFileLib
{
    /// \brief Type for the list of all Translation-Pair-Descriptions.
    using moTranslationPairList = List<MoTranslationPairInformation>;
    
    /// \brief Type for the map which holds the translation-pairs later.
    using moLookupList = Dictionary<string, string>;
    
    /// \brief Type for the 2D map which holds the translation-pairs later.
    using moContextLookupList = Dictionary<string, Dictionary<string, string>>;

    /**
     * \brief Keeps the Description of translated and original strings.
     * 
     * 
     * To load a String from the file, we need its offset and its length.
     * This struct helps us grouping this information.
     */
    public struct MoTranslationPairInformation
    {
        /// \brief Length of the Original String
        public int OrLength;

        /// \brief Offset of the Original String (absolute)
        public int OrOffset;

        /// \brief Length of the Translated String
        public int TrLength;

        /// \brief Offset of the Translated String (absolute)
        public int TrOffset;
    }

    /**
     * \brief Describes the "Header" of a .mo-File.
     * 
     * 
     * The File info keeps the header of a .mo-file and
     * a list of the string-descriptions.
     * The typedef is for the type of the string-list.
     * The constructor ensures, that all members get a nice
     * initial value.
     */
    public struct MoFileInfo
    {
        /// \brief The Magic Number, compare it to g_MagicNumber.
        public uint MagicNumber;

        /// \brief The File Version, 0 atm according to the manpage.
        public int FileVersion;

        /// \brief Number of Strings in the .mo-file.
        public int NumStrings;

        /// \brief Offset of the Table of the Original Strings
        public int OffsetOriginal;

        /// \brief Offset of the Table of the Translated Strings
        public int OffsetTranslation;

        /// \brief Size of 1 Entry in the Hashtable.
        public int SizeHashtable;

        /// \brief The Offset of the Hashtable.
        public int OffsetHashtable;

        /**
         * \brief Tells you if the bytes are reversed
         * \note When this is true, the bytes are reversed and the Magic number is like g_MagicReversed
         */
        public bool Reversed;

        /// \brief A list containing offset and length of the strings in the file.
        public moTranslationPairList TranslationPairInformation;
    }

    /**
     * \brief This class is a gettext-replacement.
     * 
     * 
     * The usage is quite simple:\n
     * Tell the class which .mo-file it shall load via
     * MoFileReader::ReadFile(). The method will attempt to load
     * the file, all translations will be stored in memory.
     * Afterwards you can lookup the strings with MoFileReader::Lookup() just
     * like you would do with gettext.
     * Additionally, you can call MoFileReader::ReadFile() for as much files as you
     * like. But please be aware, that if there are duplicated keys (original strings),
     * that they will replace each other in the lookup-table. There is no check done, if a
     * key already exists.
     * 
     * \note If you add "Lookup" to the keywords of the gettext-parser (like poEdit),
     * it will recognize the Strings loaded with an instance of this class.
     */
    public class MoFileReader
    {
        /// \brief The possible errorcodes for methods of this class
        public enum ErrorCode
        {
            /// \brief Indicated success
            Success,

            /// \brief Indicates an error
            Error,

            /// \brief The given File was not found.
            FileNotFound,

            /// \brief The file is invalid.
            FileInvalid,

            /// \brief Empty Lookup-Table (returned by ExportAsHTML())
            TableEmpty,

            /// \brief The magic number did not match
            MagicnumberNomatch,

            /**
             * \brief The magic number is reversed.
             * \note This is an error until the class supports it.
             */
            MagicnumberReversed
        }

        /// \brief The Magic Number describes the endianess of bytes on the system.
        private const uint MagicNumber = 0x950412DE;

        /// \brief If the Magic Number is Reversed, we need to swap the bytes.
        private const uint MagicReversed = 0xDE120495;

        /// \brief The character that is used to separate context strings
        private const char ContextSeparator = '\x04';

        private const string g_css = @"(
            body {
                 background-color: black;
                 color: silver;
            }
            table {
                 width: 80%;
            }
            th {
                 background-color: orange;
                 color: black;
            }
            hr {
                 color: red;
                width: 80%;
                 size: 5px;
            }
            a:link{
                color: gold;
            }
            a:visited{
                color: grey;
            }
            a:hover{
                color:blue;
            }
            .copyleft{
                 font-size: 12px;
                 text-align: center;
            })";

        /// \brief Keeps the last error as String.
        private string _error;

        // Holds the lookup-table
        private moLookupList _lookup = new moLookupList();
        private moContextLookupList _lookupContext = new moContextLookupList();

        private int _numStrings;


        /**
         * \brief Reads a .mo-file
         * \param[in] _filename The path to the file to load.
         * \return SUCCESS on success or one of the other error-codes in eErrorCode on error.
         * 
         * This is the core-feature. This method loads the .mo-file and stores
         * all translation-pairs in a map. You can access this map via the method
         * moFileReader::Lookup().
         */
        public ErrorCode ParseData(string data)
        {
            // Opening the file.
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            stream.Position = 0;
            return ReadStream(stream);
        }

        /**
         * \brief Reads a .mo-file
         * \param[in] _filename The path to the file to load.
         * \return SUCCESS on success or one of the other error-codes in eErrorCode on error.
         * 
         * This is the core-feature. This method loads the .mo-file and stores
         * all translation-pairs in a map. You can access this map via the method
         * moFileReader::Lookup().
         */
        public ErrorCode ReadFile(string filename)
        {
            if (!File.Exists(filename))
            {
                _error = $"Cannot open File {filename}";
                return ErrorCode.FileNotFound;
            }
            // Opening the file.

            using var fs = File.OpenRead(filename);
            return ReadStream(fs);
        }


        /**
         * \brief Reads data from a stream
         * \param[in] stream
         * \return SUCCESS on success or one of the other error-codes in eErrorCode on error.
         */
        private ErrorCode ReadStream(Stream stream)
        {
            // Creating a file-description.
            MoFileInfo moInfo;

            _lookup = new moLookupList();
            _lookupContext = new moContextLookupList();

            // Reference to the List inside moInfo.
            moInfo.TranslationPairInformation = new moTranslationPairList();
            var transPairInfo = moInfo.TranslationPairInformation;

            // Read in all the 4 bytes of fire-magic, offsets and stuff...
            try
            {
                moInfo.MagicNumber = stream.ReadUInt(4);
                moInfo.FileVersion = stream.ReadInt(4);
                moInfo.NumStrings = stream.ReadInt(4);
                moInfo.OffsetOriginal = stream.ReadInt(4);
                moInfo.OffsetTranslation = stream.ReadInt(4);
                moInfo.SizeHashtable = stream.ReadInt(4);
                moInfo.OffsetHashtable = stream.ReadInt(4);
            }
            catch (Exception e)
            {
                _error = e.Message;
                return ErrorCode.FileInvalid;
            }

            if (moInfo.MagicNumber == 0 || moInfo.NumStrings == 0)
            {
                _error = "Stream bad during reading. The .mo-file seems to be invalid or has bad descriptions!";
                return ErrorCode.FileInvalid;
            }

            // Checking the Magic Number
            if (MagicNumber != moInfo.MagicNumber)
            {
                if (MagicReversed != moInfo.MagicNumber)
                {
                    _error = "The Magic Number does not match in all cases!";
                    return ErrorCode.MagicnumberNomatch;
                }

                moInfo.Reversed = true;
                _error = "Magic Number is reversed. We do not support this yet!";
                return ErrorCode.MagicnumberReversed;
            }

            // Now we search all Length & Offsets of the original strings
            for (var i = 0; i < moInfo.NumStrings; i++)
                try
                {
                    var str = new MoTranslationPairInformation
                    {
                        OrLength = stream.ReadInt(4),
                        OrOffset = stream.ReadInt(4)
                    };
                    transPairInfo.Add(str);
                }
                catch
                {
                    _error = "Stream bad during reading. The .mo-file seems to be invalid or has bad descriptions!";
                    return ErrorCode.FileInvalid;
                }

            // Get all Lengths & Offsets of the translated strings
            // Be aware: The Descriptors already exist in our list, so we just mod. refs from the deque.
            for (var i = 0; i < moInfo.NumStrings; i++)
                try
                {
                    var str = transPairInfo[i];
                    str.TrLength = stream.ReadInt(4);
                    str.TrOffset = stream.ReadInt(4);
                    transPairInfo[i] = str;
                }
                catch
                {
                    _error = "Stream bad during reading. The .mo-file seems to be invalid or has bad descriptions!";
                    return ErrorCode.FileInvalid;
                }

            // Normally you would read the hash-table here, but we don't use it. :)

            // Now to the interesting part, we read the strings-pairs now
            for (var i = 0; i < moInfo.NumStrings; i++)
            {
                // We need a length of +1 to catch the trailing \0.
                var orLength = transPairInfo[i].OrLength;
                var trLength = transPairInfo[i].TrLength;

                var orOffset = transPairInfo[i].OrOffset;
                var trOffset = transPairInfo[i].TrOffset;

                string original, translation;

                // Original
                try
                {
                    stream.Seek(orOffset, SeekOrigin.Begin);
                    original = stream.ReadString(orLength);
                }
                catch
                {
                    _error = "Stream bad during reading. The .mo-file seems to be invalid or has bad descriptions!";
                    return ErrorCode.FileInvalid;
                }

                // Translation
                try
                {
                    stream.Seek(trOffset, SeekOrigin.Begin);
                    translation = stream.ReadString(trLength);
                }
                catch
                {
                    _error = "Stream bad during reading. The .mo-file seems to be invalid or has bad descriptions!";
                    return ErrorCode.FileInvalid;
                }

                var ctxSeparator = original.IndexOf(ContextSeparator);

                // Store it in the map.
                if (ctxSeparator == -1)
                {
                    _lookup[original] = translation;
                    _numStrings++;
                }
                else
                {
                    var l = original.Split(ContextSeparator);
                    if (l.Length != 2)
                    {
                        _error = "Failed to split string";
                        return ErrorCode.Error;
                    }

                    var context = l[0];
                    var id = l[1];

                    if (!_lookupContext.ContainsKey(context))
                        _lookupContext.Add(context, new Dictionary<string, string>());

                    _lookupContext[context][id] = translation;
                    _numStrings++;
                }
            }

            // Done :)
            return ErrorCode.Success;
        }

        /**
         * \brief Returns the searched translation or returns the input.
         * \param[in] id The id of the translation to search for.
         * \return The value you passed in via _id or the translated string.
         */
        public string Lookup(string id)
        {
            if (_lookup.Count == 0) return id;
            return _lookup.ContainsKey(id) ? _lookup[id] : id;
        }

        /**
         * \brief Returns the searched translation or returns the input, restricted to the context given by context.
         * See https://www.gnu.org/software/gettext/manual/html_node/Contexts.html for more info.
         * \param[in] context Restrict to the context given.
         * \param[in] id The id of the translation to search for.
         * \return The value you passed in via _id or the translated string.
         */
        public string LookupWithContext(string context, string id)
        {
            if (_lookupContext.Count == 0 ||
                !_lookupContext.ContainsKey(context) ||
                !_lookupContext[context].ContainsKey(id))
                return id;

            return _lookupContext[context][id];
        }

        /// \brief Returns the Error Description.
        public string GetErrorDescription()
        {
            return _error;
        }

        /// \brief Empties the Lookup-Table.
        public void ClearTable()
        {
            _lookup.Clear();
            _lookupContext.Clear();
            _numStrings = 0;
        }

        /**
         * \brief Returns the Number of Entries in our Lookup-Table.
         * \note The mo-File-table always contains an empty msgid, which contains informations
         * about the tranlsation-project. So the real number of strings is always minus 1.
         */
        public int GetNumStrings()
        {
            return _numStrings;
        }

        /**
         * \brief Exports the whole content of the .mo-File as .html
         * \param[in] infile The .mo-File to export.
         * \param[in] filename Where to store the .html-file. If empty, the path and filename of the _infile with .html appended.
         * \param[in,out] css The css-script for the visual style of the
         * file, in case you don't like mine ;).
         * \see g_css for the possible and used css-values.
         */
        private static ErrorCode ExportAsHtml(string infile, string filename = "", string css = g_css)
        {
            // // Read the file
            // moFileReader reader;
            // moFileReader::eErrorCode r = reader.ReadFile(infile.c_str());
            // if (r != moFileReader::EC_SUCCESS)
            // {
            //     return r;
            // }
            //
            // if (reader.lookup.empty())
            // {
            //     return moFileReader::EC_TABLEEMPTY;
            // }
            //
            // // Beautify Output
            // string fname;
            // unsigned int pos = infile.find_last_of(MO_PATHSEP);
            // if (pos != string::npos) {
            //     fname = infile.substr(pos + 1, infile.length());
            // }
            // else
            // {
            //     fname = infile;
            // }
            //
            // // if there is no filename given, we set it to the .mo + html, e.g. test.mo.html
            // string htmlfile(filename);
            // if (htmlfile.empty())
            // {
            //     htmlfile = infile + string(".html");
            // }
            //
            // // Ok, now prepare output.
            // std::ofstream stream(htmlfile.c_str());
            // if (stream.is_open())
            // {
            //     stream << R"(<!DOCTYPE HTML PUBLIC " - //W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">)"
            //         << std::endl;
            //     stream << "<html><head><style type=\"text/css\">\n" << std::endl;
            //     stream << css << std::endl;
            //     stream << "</style>" << std::endl;
            //     stream << R"(<meta http-equiv="content - type" content="text / html;
            //     charset = utf - 8">)" << std::endl;
            //     stream << "<title>Dump of " << fname << "</title></head>" << std::endl;
            //     stream << "<body>" << std::endl;
            //     stream << "<center>" << std::endl;
            //     stream << "<h1>" << fname << "</h1>" << std::endl;
            //     stream << R"(<table border="1"><th colspan="2">Project Info</th>)" << std::endl;
            //
            //     stringstream parsee;
            //     parsee << reader.Lookup("");
            //
            //     while (!parsee.eof())
            //     {
            //         char buffer[1024];
            //         parsee.getline(buffer, 1024);
            //         string name;
            //         string value;
            //
            //         reader.GetPoEditorString(buffer, name, value);
            //         if (!(name.empty() || value.empty()))
            //         {
            //             stream << "<tr><td>" << name << "</td><td>" << value << "</td></tr>" << std::endl;
            //         }
            //     }
            //
            //     stream << "</table>" << std::endl;
            //     stream << "<hr noshade/>" << std::endl;
            //
            //     // Now output the content
            //     stream << R"(<table border="1"><th colspan="2">Content</th>)" << std::endl;
            //     for ( const auto 
            //     &it : reader.lookup)
            //     {
            //         if (!it.first.empty()) // Skip the empty msgid, its the table we handled above.
            //         {
            //             stream << "<tr><td>" << it.first << "</td><td>" << it.second << "</td></tr>" << std::endl;
            //         }
            //     }
            //     stream << "</table><br/>" << std::endl;
            //
            //     // Separate tables for each context
            //     for ( const auto 
            //     &it : reader.lookup_context)
            //     {
            //         stream << R"(<table border="1"><th colspan="2">)" << it.first << "</th>" << std::endl;
            //         for ( const auto 
            //         &its : it.second)
            //         {
            //             stream << "<tr><td>" << its.first << "</td><td>" << its.second << "</td></tr>" << std::endl;
            //         }
            //         stream << "</table><br/>" << std::endl;
            //     }
            //
            //     stream << "</center>" << std::endl;
            //     stream <<
            //         "<div class=\"copyleft\">File generated by <a href=\"https://github.com/AnotherFoxGuy/MofileReader\" "
            //     "target=\"_blank\">moFileReaderSDK</a></div>"
            //         << std::endl;
            //     stream << "</body></html>" << std::endl;
            //     stream.close();
            // }
            // else
            // {
            //     return moFileReader::EC_FILENOTFOUND;
            // }

            return ErrorCode.Success;
        }

        /**
         * \brief Swap the endianness of a 4 byte WORD.
         * \param[in] in The value to swap.
         * \return The swapped value.
         */
        private long SwapBytes(long inp)
        {
            var b0 = (inp >> 0) & 0xff;
            var b1 = (inp >> 8) & 0xff;
            var b2 = (inp >> 16) & 0xff;
            var b3 = (inp >> 24) & 0xff;

            return (b0 << 24) | (b1 << 16) | (b2 << 8) | b3;
        }

        // Replaces < with ( to satisfy html-rules.
        private static void MakeHtmlConform(string _inout)
        {
            // string temp = _inout;
            // for (unsigned int i = 0;
            // i < temp.length();
            // i++)
            // {
            //     if (temp[i] == '>')
            //     {
            //         _inout.replace(i, 1, ")");
            //     }
            //
            //     if (temp[i] == '<')
            //     {
            //         _inout.replace(i, 1, "(");
            //     }
            // }
        }

        // Extracts a value-pair from the po-edit-information
        private bool GetPoEditorString(char _buffer, string name, string value)
        {
            // string line(_buffer);
            // size_t first = line.find_first_of(':');
            //
            // if (first != string::npos)
            // {
            //     _name = line.substr(0, first);
            //     _value = line.substr(first + 1, line.length());
            //
            //     // Replace <> with () for Html-Conformity.
            //     MakeHtmlConform(_value);
            //     MakeHtmlConform(_name);
            //
            //     // Remove spaces from front and end.
            //     Trim(_value);
            //     Trim(_name);
            //
            //     return true;
            // }
            return false;
        }

        // Removes spaces from front and end.
        private static void Trim(string _in)
        {
            while (_in[0] == ' ') _in = _in.Substring(1, _in.Length);

            while (_in[_in.Length] == ' ') _in = _in.Substring(0, _in.Length - 1);
        }
    }

    /**
     * \brief Convenience Class
     * 
     * 
     * This class derives from moFileReader and builds a singleton to access its methods
     * in a global manner.
     * \note This class is a Singleton. Please access it via moFileReaderSingleton::GetInstance()
     * or use the provided wrappers:\n
     * - moReadMoFile()
     * - _()
     * - moFileClearTable()
     * - moFileGetErrorDescription()
     * - moFileGetNumStrings();
     */
    public class MoFileReaderSingleton : MoFileReader
    {
        private static readonly Lazy<MoFileReader> TheOneAndOnly =
            new(() => new MoFileReader());

        // Private Contructor and Copy-Constructor to avoid
        // that this class is instanced.
        private MoFileReaderSingleton()
        {
        }

        /**
         * \brief Singleton-Accessor.
         * \return A static instance of moFileReaderSingleton.
         */
        public static MoFileReader Instance => TheOneAndOnly.Value;
    }

    public static class ConvenienceClasses
    {
        /**
         * \brief Reads the .mo-File.
         * \param[in] _filename The path to the file to use.
         * \see moFileReader::ReadFile() for details.
         */
        public static MoFileReader.ErrorCode MoReadMoFile(string filename) =>
            MoFileReaderSingleton.Instance.ReadFile(filename);

        /**
         * \brief Looks for the spec. string to translate.
         * \param[in] id The string-id to search.
         * \return The translation if found, otherwise it returns id.
         */
        public static string _(string id) => MoFileReaderSingleton.Instance.Lookup(id);

        /// \brief Resets the Lookup-Table.
        public static void MoFileClearTable() => MoFileReaderSingleton.Instance.ClearTable();

        /// \brief Returns the last known error as string or an empty class.
        public static string MoFileGetErrorDescription() => MoFileReaderSingleton.Instance.GetErrorDescription();

        /// \brief Returns the number of entries loaded from the .mo-File.
        public static int MoFileGetNumStrings() => MoFileReaderSingleton.Instance.GetNumStrings();
    }

    internal static class StreamExtensions
    {
        public static int ReadInt(this Stream stream, int length)
        {
            var bytes = new byte[8];
            stream.Read(bytes, 0, length);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static uint ReadUInt(this Stream stream, int length)
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, length);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static string ReadString(this Stream stream, int length)
        {
            var bytes = new byte[length];
            stream.Read(bytes, 0, length);
            return Encoding.Default.GetString(bytes);
        }
    }
}