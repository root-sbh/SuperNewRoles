using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUs.Data;
using Il2CppSystem.CodeDom;
using UnityEngine.UIElements.UIR;

namespace SuperNewRoles.Modules;

public static class ModTranslation
{
    // 一番左と一行全部
    public static Dictionary<string, string[]> dictionary = new();
    public static Dictionary<string, string[]> AprilDictionary = null;
    private static readonly HashSet<string> outputtedStr = new();
    public static string GetString(string key)
    {
        Dictionary<string, string[]> currentTransDict = AprilDictionary != null ? AprilDictionary : dictionary;

        // アモアス側の言語読み込みが完了しているか ? 今の言語 : 最後の言語
        SupportedLangs langId = DestroyableSingleton<TranslationController>.InstanceExists ? FastDestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID : DataManager.Settings.Language.CurrentLanguage;

        if (!currentTransDict.TryGetValue(key, out string[] values)) return key; // keyが辞書にないならkeyのまま返す

        if (langId is SupportedLangs.SChinese or SupportedLangs.TChinese)
        {
            if (langId == SupportedLangs.SChinese && (values.Length < 4 || values[3] == ""))
            { //簡体中国語がない場合英語で返す
                if (!outputtedStr.Contains(key))
                    Logger.Info($"SChinese not found:{key}", "ModTranslation");
                outputtedStr.Add(key);
                return values[1];
            }

            if (langId == SupportedLangs.TChinese && (values.Length < 5 || values[4] == ""))
            { //繁体中国語がない場合英語で返す
                if (!outputtedStr.Contains(key))
                    Logger.Info($"TChinese not found:{key}", "ModTranslation");
                outputtedStr.Add(key);
                return values[1];
            }
        }

        return langId switch
        {
            SupportedLangs.English => values[1], // 英語
            SupportedLangs.Japanese => values[2],// 日本語
            SupportedLangs.SChinese => values[3],// 簡体中国語
            SupportedLangs.TChinese => values[4],// 繁体中国語
            _ => values[1] // それ以外は英語
        };
    }

    public static string GetString(string key, params object[] args) => string.Format(GetString(key), args);

    public static string GetString<T1, T2, T3, T4> (string key, T1? value1, T2? value2, T3? value3, T4? value4)
    {
        switch (key)
        {
            case "":
                if (value1 is not int) return "";
                return $"{value1}";
        }
        return string.Format(GetString(key), value1, value2, value3, value4);
    }

    /// <summary>
    /// 翻訳語の文章から翻訳キーを取得する。
    /// CustomOptionで追加しているカラータグは先に外してください。
    /// </summary>
    /// <param name="value">keyを取得したい翻訳後の文</param>
    /// <returns>
    /// string : keyが存在 => key / keyが存在しない => 引数をそのまま返す
    /// bool : true => keyの取得に成功 / false => keyの取得に失敗
    /// </returns>
    internal static (string[], bool) GetTranslateKey(string value)
    {
        SupportedLangs langId = TranslationController.InstanceExists ? TranslationController.Instance.currentLanguage.languageID : DataManager.Settings.Language.CurrentLanguage;

        int index = langId switch
        {
            SupportedLangs.English => 1,
            SupportedLangs.Japanese => 2,
            SupportedLangs.SChinese => 3,
            SupportedLangs.TChinese => 4,
            _ => 1,
        };

        string[] keys = dictionary.Where(x => x.Value[index].Equals(value)).Select(x => x.Key).ToArray(); // 指定された翻訳を有するkeyをすべて取得する。
        if (0 == keys.Length) // 翻訳キー取得失敗
        {
            Logger.Info($"key not found:{value}", "ModTranslation");
            return (value.Split(""), false);
        }
        else // 翻訳キー取得成功
        {
            Logger.Info($"key could be found : ( {string.Join(", ", keys)} )", "ModTranslation");
            return (keys, true);
        }
    }

    public static void LoadCsv()
    {
        var fileName = Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperNewRoles.Resources.Translate.csv");

        //csvを開く
        StreamReader sr = new(fileName);

        var i = 0;
        //1行ずつ処理
        while (!sr.EndOfStream)
        {
            try
            {
                // 行ごとの文字列
                string line = sr.ReadLine();

                // 行が空白 戦闘が*なら次の行に
                if (line == "" || line[0] == '#') continue;

                //カンマで配列の要素として分ける
                string[] values = line.Split(',');

                // 配列から辞書に格納する
                List<string> valuesList = new();
                foreach (string vl in values)
                {
                    valuesList.Add(vl.Replace("\\n", "\n").Replace("，", ","));
                }
                dictionary.Add(values[0], valuesList.ToArray());
                i++;
            }
            catch
            {
                Logger.Error($"Error: Loading Translate.csv Line:{i}", "ModTranslation");
            }
        }
    }
}