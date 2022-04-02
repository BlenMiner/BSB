using System;
using System.Collections.Generic;
using System.Text;
using Bestcode.MathParser;
using UnityEngine;

public class Formula
{
    public bool IsValid {get; private set;}

    private string m_parsedFormula;

    public string Error {get; private set;} = string.Empty;

    MathParser m_parser = new MathParser();

    List<DatasetReference> m_variables;

    object[] m_args;

    public string RawText { get; private set; }

    struct DatasetReference
    {
        public Dataset Source;

        public string Attribute;
    }

    public Formula(DatasetAutocompletion datasetInfo, string value, string defaultV = "")
    {
        if (value == "") value = defaultV;
        RawText = value;

        m_variables = new List<DatasetReference>();

        IsValid = Parse(datasetInfo, value);
    }

    bool Parse(DatasetAutocompletion datasetInfo, string expression)
    {
        StringBuilder parsedExpr = new StringBuilder();
        StringBuilder expr = new StringBuilder();

        bool validSyntax = true;

        m_variables.Clear();

        int expressionIdx = 0;

        void ProcessVar(string variable)
        {
            switch (variable.ToLower())
            {
                case "sqrt": 
                case "pow":
                case "min": 
                case "max":
                case "if":
                case "switch":
                case "sin":
                case "cos":
                case "tan":
                case "sum":
                case "round":
                case "floor":
                case "ceiling":
                case "pi":
                parsedExpr.Append(variable);
                return;
            }
            string[] info = variable.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            parsedExpr.Append($"{(char)('a' + expressionIdx++)}");

            if (info.Length == 2)
            {
                var entry = new DatasetReference{
                    Attribute = info[1],
                    Source = datasetInfo.GetDataset(info[0])
                };

                if (entry.Source == null)
                {
                    Error = $"Database doesn't exist: '{info[0]}'";
                    validSyntax = false;
                }

                m_variables.Add(entry);
            }
            else
            {
                Error = $"Invalid syntax: '{variable}'";
                validSyntax = false;
                m_variables.Add(default);
            }
        }
        
        for (int i = 0; i < expression.Length; ++i)
        {
            char c = expression[i];
            bool isValidStartOfWord = char.IsLetter(c);

            if (expr.Length == 0)
            {
                if (isValidStartOfWord)
                    expr.Append(c);
                else parsedExpr.Append(c);
            }
            else if (!char.IsLetterOrDigit(c) && c != '.')
            {
                // Word finished
                string word = expr.ToString();
                expr.Clear();
    
                ProcessVar(word);

                parsedExpr.Append(c);
            }
            else expr.Append(c);
        }

        if (expr.Length != 0)
            ProcessVar(expr.ToString());

        m_parsedFormula = parsedExpr.ToString();
        return validSyntax;
    }

    public bool ComputeMin(out float value)
    {
        value = 0f;

        try
        {
            value = ComputeExprMin();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool ComputeMax(out float value)
    {
        value = 0f;

        try
        {
            value = ComputeExprMax();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    float ComputeExprMin()
    {
        var expr = m_parsedFormula;
        if (m_parser.Expression != expr)
            m_parser.Expression = expr;

        for(int i = 0; i < m_variables.Count; ++i)
        {
            var variable = m_variables[i];
            m_parser.SetVariable(((char)('a' + i)).ToString(), variable.Source.GetMinPossibleValue(variable.Attribute));
        }

        return (float)m_parser.ValueAsDouble;
    }

    float ComputeExprMax()
    {
        var expr = m_parsedFormula;
        if (m_parser.Expression != expr)
            m_parser.Expression = expr;

        for(int i = 0; i < m_variables.Count; ++i)
        {
            var variable = m_variables[i];
            m_parser.SetVariable(((char)('a' + i)).ToString(), variable.Source.GetMaxPossibleValue(variable.Attribute));
        }

        return (float)m_parser.ValueAsDouble;
    }

    float ComputeExpr(string depId, float time)
    {
        var expr = m_parsedFormula;
        if (m_parser.Expression != expr)
            m_parser.Expression = expr;

        for(int i = 0; i < m_variables.Count; ++i)
        {
            var variable = m_variables[i];

            if (variable.Source.GetData(depId, variable.Attribute, time, out var v))
                m_parser.SetVariable(((char)('a' + i)).ToString(), v);
            else throw new Exception("Missing value.");
        }

        return (float)m_parser.ValueAsDouble;
    }

    public bool Compute(string depId, float time, out float value)
    {
        value = 0f;

        try
        {
            value = ComputeExpr(depId, time);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
