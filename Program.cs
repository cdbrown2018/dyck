/**
    To compile: dotnet build
    To run: dotnet run in.txt > out.txt

    <sentence>    ::= <subject><verb_phrase><object>
    <subject>     ::= <noun_phrase>
    <verb-phrase> ::= <verb> | <verb> <adv>
    <object>      ::= <noun_phrase>
    <verb>        ::= learn | leave | serve
    <adv>         ::= yesterday | today | tomorrow
    <noun_phrase> ::= [<adj_phrase>] <noun> [<prep_phrase>]
    <noun>        ::= faith | hope | charity
    <adj_phrase>  ::= <adj> | <adj> <adj_phrase>
    <adj>         ::= humble | patient | prudent
    <prep_phrase> ::= <prep> <noun_phrase>
    <prep>        ::= of | at | with
**/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Duck
{
    class Program
    {
        private static List<string> lexemes = new List<string> {"learn", "leave", "serve", "yesterday", "today", "tomorrow", "faith", "hope", "charity",
                                                    "humble", "patient", "prudent", "of", "at", "with"};
        private static string currentLexeme;
        private static string diagram;
        private static int index = -1;
        private static List<string> input = new List<string>(); 
        private static bool error = false;

        private static List<string> verbStrings = new List<string> { "learn", "leave", "serve" };
        private static List<string> advStrings = new List<string> { "yesterday", "today", "tomorrow" };
        private static List<string> nounStrings = new List<string> { "faith", "hope", "charity" };
        private static List<string> adjStrings = new List<string> { "humble", "patient", "prudent" };
        private static List<string> prepStrings = new List<string> { "of", "at", "with" };

        /// <summary>
        /// Main runner for parsing sentences
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            StreamReader streamReader = null;
            if (args.Length > 0)
            {
                streamReader = new StreamReader(args[0]);
                Console.SetIn(streamReader);
            }
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                input = line.Split(' ').ToList();
                if (!ValidateLexemes())
                {
                    System.Console.WriteLine($"\"{line}\" contains lexical units which are not lexemes and, thus, is not an expression.");
                }
                else {
                    diagram = "";
                    nextLexeme();
                    sentence();

                    if (error) Console.WriteLine($"\"{line}\" is not a sentence.");
                    else Console.WriteLine($"\"{diagram}\" is a sentence.");
                }
                error = false; // reset error status for next candidate
                index = -1; // reset index for sentences
                currentLexeme = ""; // reset currentLexeme
            }

            if (streamReader != null) streamReader.Close();
        }

        /// <summary>
        /// The root or start symbol of the given sentence
        /// </summary>
        private static void sentence()
        {
            // <sentence> ::= <subject><verb_phrase><object>
            diagram += " (";
            subject();
            if (!error && currentLexeme != " ") { verb_phrase(); }
            if (!error && currentLexeme != " ") { object_p(); }
            diagram += ") ";
        }

        /// <summary>
        /// Checks this portion of the candidate sentence for a subject
        /// </summary>
        private static void subject()
        {
            // <subject> ::= <noun_phrase>
            diagram += " (";            
            noun_phrase();
            diagram += ") ";
        }

        /// <summary>
        /// Checks this portion of the candidate sentence for a verb phrase
        /// </summary>
        private static void verb_phrase()
        {
            // <verb-phrase> ::= <verb> | <verb> <adv>
            diagram += "(";
            verb();
            if (error) return;
            nextLexeme();
            adv();
            if (!error) { diagram += " "; nextLexeme(); }
            error = false;
            diagram += ") ";
        }

        /// <summary>
        /// Checks this portion of the candidate sentence for an object
        /// </summary>
        private static void object_p()
        {
            // <object> ::= <noun_phrase>
            diagram += " (";
            noun_phrase();
            diagram += ") ";
        }

        /// <summary>
        /// Checks this portion of the candidate sentence for a noun phrase
        /// </summary>
        private static void noun_phrase()
        {
            // <noun_phrase> ::= [<adj_phrase>] <noun> [<prep_phrase>]

            diagram += " (";
            // if noun isn't present, adj_phrase is required.
            if (!nounStrings.Contains(currentLexeme)) adj_phrase();

            noun();

            if (error) return;
            if (!error && currentLexeme != " ") {nextLexeme(); prep_phrase();}
            error = false;
            diagram += ") ";
        }

        /// <summary>
        /// Checks this portion of the candidate sentence for an adjective phrase
        /// </summary>
        private static void adj_phrase()
        {
            //<adj_phrase> ::= <adj> | <adj> <adj_phrase>

            diagram += " (";
            // A single adjective is required. If it errors, it's a legit error
            // Beyond that, it doesn't matter, we really just need to get to the next non-adjective
            // That error isn't *real* because it just means we got to the end of the chain of adjectives
            // So we set the error back to false
            adj();
            if (error) { diagram = diagram.Substring(0, diagram.Length - 2); return; }
            optional_adj_phrase();
            error = false;
            diagram += ") ";
        }

        /// <summary>
        /// A helper for adj_phrase. Allows for several calls
        /// </summary>
        private static void optional_adj_phrase() {
            nextLexeme();
            diagram += " (";
            adj();
            if (error) { diagram = diagram.Substring(0, diagram.Length - 2); return; }
            if (!error) adj_phrase();
            diagram += ") ";
        }

        /// <summary>
        /// Checks this portion of the candidate sentence for a prepositional phrase
        /// </summary>
        private static void prep_phrase()
        {
            // <prep_phrase> ::= <prep> <noun_phrase>
            diagram += " (";
            prep();
            if (error) { diagram = diagram.Substring(0, diagram.Length - 2); return; }
            if (!error) { nextLexeme(); noun_phrase(); }
            diagram += ") ";
        }

        /// <summary>
        /// Determines whether the current lexeme is a verb
        /// </summary>
        private static void verb()
        {
            //<verb> ::= learn | leave | serve
            error = !(verbStrings.Contains(currentLexeme));
        }

        /// <summary>
        /// Determines whether the current lexeme is an adverb
        /// </summary>
        private static void adv()
        {
            // <adv> ::= yesterday | today | tomorrow
            error = !(advStrings).Contains(currentLexeme);
        }

        /// <summary>
        /// Determines whether the current lexeme is a noun
        /// </summary>
        private static void noun()
        {
            //<noun> ::= faith | hope | charity
            error = !(nounStrings).Contains(currentLexeme);
        }

        /// <summary>
        /// Determines whether the current lexeme is a preposition
        /// </summary>
        private static void prep()
        {
            // <prep> ::= of | at | with
            error = !(prepStrings).Contains(currentLexeme);
        }

        /// <summary>
        /// Determines whether the current lexeme is an adjective
        /// </summary>
        private static void adj() {
            error = !(adjStrings).Contains(currentLexeme);
        }

        /// <summary>
        /// Gets the next lexeme in a series
        /// </summary>
        private static void nextLexeme()
        {
            diagram += currentLexeme;
            index++;
            if (index < input.Count()) currentLexeme = input[index];
            else currentLexeme = " ";
        }

        /// <summary>
        /// Validates that all words in the input string are valid
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static bool ValidateLexemes()
        {
            return input.Distinct().Intersect(lexemes).Count() == input.Distinct().Count();
        }
    }
}