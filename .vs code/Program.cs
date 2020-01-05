using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangComp
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, int> wordScores = new Dictionary<string, int>();
            List<string> wordList = new List<string>();
            List<string> wordHierarchy = new List<string>();
            Dictionary<string, int> word2wordHierarchy = new Dictionary<string, int>();
            Dictionary<string, int> symbol2val = new Dictionary<string, int>();
            List<string> wordIndex = new List<string>();
            string[] symbols;
            string textFilePath = "lcText.txt";
            string symbolFilePath = "lcSymbols.txt";
            string outputFilePath = "lcOutput.txt";
            string wordListFilePath = "lcWordList.txt";
            string decopressionFilePath = "lcDecompressed.txt";
            string blackListFilePath = "lcSymbolBlackList.txt";
            string uncompressedSymbol = "Δ";
            importSymbols();
            comandInput();
            void importSymbols()
            {
                symbols = System.IO.File.ReadAllLines(symbolFilePath);
                for (int i = 0; i < symbols.Length; i++) {
                    symbol2val.Add(symbols[i], i);
                }
            }
            void comandInput()
            {
                string command = Console.ReadLine();
                if (command.Contains("-read")) {
                    readText("4read");
                    hierarchyProcessing();
                    comandInput();
                }
                else if (command.Contains("-write")) {
                    readText("4write");
                    wordCompressing();
                    wordWriting();
                    createWordListFile();
                    comandInput();
                } else if (command.Contains("-dcmp")) {
                    readText("4decompress");
                    processigDecompression();
                    comandInput();
                }
            }
            #region readingText
            void readText(string state)
            {
                string text = System.IO.File.ReadAllText(textFilePath).Replace(Environment.NewLine, "");
                string word = "";
                if (state == "4write" || state == "4decompress") {
                    wordIndex.Clear();
                }
                for (int i = 0; i < text.Length; i++) {
                    #region 4Read
                    if (state == "4read") {
                        if ((text[i] == ' ' || onBlackList(text[i]))) {
                            if (word != "") {
                                wordProcessing(word);
                                word = "";
                            }
                        }
                        else {
                            word += text[i];
                        }
                    }
                    #endregion
                    #region 4Write
                    if (state == "4write") {
                        if (text[i] == ' ') {
                            wordIndexing(word);
                            word = "";
                        } else if (onBlackList(text[i])) {
                            if (word == "") {
                                word += uncompressedSymbol + text[i];
                            }
                            else if (onBlackList(word[word.Length - 1])) {
                                word += text[i];
                            }
                            if (!onBlackList(word[word.Length - 1])) {
                                wordIndexing(word);
                                word = "";
                                word += uncompressedSymbol + text[i];
                            }
                            if (!onBlackList(text[i+1]) && text[i+1] != ' ') {
                                word += text[i];
                                wordIndexing(word);
                                word = "";
                            }
                        } else {
                            word += text[i];
                        }
                    }
                    #endregion
                    #region 4decompress
                    if (state == "4decompress") {
                        if (text[i] == ' ') {
                            if (word != "") {
                                wordIndexing(word);
                                word = "";
                            }
                        }
                        else {
                            word += text[i];
                        }
                    }
                    #endregion
                }
            }
            void wordIndexing(string word)
            {
                wordIndex.Add(word);
            }
            bool onBlackList(char character)
            {
                bool isOnBlackList = false;
                string[] blackListSymbols = System.IO.File.ReadAllLines(blackListFilePath);
                for (int i = 0;i < blackListFilePath.Length; i++) {
                    if (Convert.ToString(character) == blackListSymbols[i]) {
                        isOnBlackList = true;
                    }
                }
                return isOnBlackList;
            }
            #endregion
            #region processWordDecompression
            void processigDecompression()
            {
                string[] wordList4Decompression = System.IO.File.ReadAllLines(wordListFilePath);
                string text = "";
                for(int i = 0;i < wordIndex.Count(); i++) {
                    if (!wordIndex[i].Contains(uncompressedSymbol)) {
                        text += wordList4Decompression[TenFromHBase(wordIndex[i])].Replace(Environment.NewLine,"") + " ";
                    } else {
                        text += wordIndex[i].Replace(uncompressedSymbol,"") + " ";
                    }
                    if (i % 20 == 0) {
                        text += Environment.NewLine;
                    }
                }
                System.IO.File.WriteAllText(decopressionFilePath,text);
            }
            #endregion
            #region processWordsWrite
            void wordCompressing()
            {
                for (int i = 0;i < wordIndex.Count(); i++) {
                    if (!wordIndex[i].Contains(uncompressedSymbol)) {
                        wordIndex[i] = TenToHBase(word2wordHierarchy[wordIndex[i]]);
                    }
                }
            }
            void wordWriting()
            {
                string text = "";
                for (int i = 0;i < wordIndex.Count(); i++) {
                    text += wordIndex[i] + " ";
                    if (i % 20 == 0 && i != 0) {
                        text += Environment.NewLine;
                    }
                }
                System.IO.File.WriteAllText(outputFilePath,text);
            }
            void createWordListFile()
            {
                string words = "";
                for (int i = 0;i < wordHierarchy.Count(); i++) {
                    words += wordHierarchy[i] + Environment.NewLine;
                }
                System.IO.File.WriteAllText(wordListFilePath,words);
            }
            #endregion
            #region processWordsRead
            void wordProcessing(string word)
            {
                if (wordList.Contains(word)) {
                    wordScores[word] += word.Length;
                }
                else {
                    wordList.Add(word);
                    wordScores.Add(word, word.Length);
                }
            }
            void hierarchyProcessing()
            {
                wordHierarchy.Clear();
                word2wordHierarchy.Clear();
                for (int i = 0; i < wordList.Count; i++) {
                    if (wordHierarchy.Count == 0) {
                        wordHierarchy.Add(wordList[i]);
                    }
                    else if (wordHierarchy.Count == 1){
                        if (wordScores[wordList[i]] > wordScores[wordHierarchy[0]]) {
                            dropArray(wordHierarchy,0);
                            wordHierarchy[0] = wordList[i];
                        }else {
                            wordHierarchy.Add(wordList[i]);
                        }
                    } else {
                        if (wordScores[wordList[i]] > wordScores[wordHierarchy[0]]) {
                            dropArray(wordHierarchy,0);
                            wordHierarchy[0] = wordList[i];
                        } else if (wordScores[wordList[i]] <= wordScores[wordHierarchy[wordHierarchy.Count - 1]]) {
                            wordHierarchy.Add(wordList[i]);
                        }else {
                            for (int j = wordHierarchy.Count-1;j > 0; j--) {
                                if (wordScores[wordList[i]] >= wordScores[wordHierarchy[j]] && wordScores[wordList[i]] <= wordScores[wordHierarchy[j-1]]) {
                                    dropArray(wordHierarchy,j);
                                    wordHierarchy[j] = wordList[i];
                                    break;
                                }
                            }
                        }
                        
                    }
                }
                for (int i = 0;i < wordHierarchy.Count; i++) {
                    word2wordHierarchy.Add(wordHierarchy[i],i);
                }
                List<string> nonShared = new List<string>();
                for(int i = 0; i < wordList.Count(); i++) {
                    if (!wordHierarchy.Contains(wordList[i])) {
                        nonShared.Add(wordList[i]);
                    }
                }
                if(nonShared.Count != 0) {
                    Console.WriteLine("An error has occured future writing may not work!");
                    Console.WriteLine("Details: Pre sorted word list and the sorted one don't have all elements in common.");
                }
            }
            void dropArray(List<string> list,int dropPoint)
            {
                list.Add(list[list.Count-1]);
                for (int i = list.Count - 2;i > dropPoint;i--) {
                    wordHierarchy[i] = wordHierarchy[i-1];
                }
            }
            #endregion
            #region conversionFunctions
            string TenToHBase(int number){
                List<int> rests = new List<int>();
                string numberHBase = "";
                if (number == 0) {
                    numberHBase = "!";
                }
                while (number != 0) {
                    rests.Add(number % symbols.Length);
                    number /= symbols.Length;
                }
                for (int i = rests.Count - 1;i > -1; i--) {
                    numberHBase += symbols[rests[i]];
                }
                return numberHBase;
            }
            int TenFromHBase(string numberHBase)
            {
                char[] numberHbaseCharArray = numberHBase.ToCharArray();
                Array.Reverse(numberHbaseCharArray);
                numberHBase = new string(numberHbaseCharArray);
                int number = 0;
                for (int i = numberHBase.Length - 1; i > -1;i--) {
                    number += (int)(Math.Pow(symbols.Length,i) * symbol2val[Convert.ToString(numberHBase[i])]);
                }
                return number;
            }
            #endregion
        }
    }
}
