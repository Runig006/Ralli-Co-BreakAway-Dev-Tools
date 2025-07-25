using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class MathMiddleware : RDRSReaderBase
{
    [SerializeField] private RDRSReaderBase[] sources;
    [SerializeField] private string expression = "{0} * 0.5 + {1}";


    private List<string> steps;

    private void Awake()
    {
        this.steps = this.ToShunting(this.expression);
    }

    public override object GetValue()
    {
    #if UNITY_EDITOR
        this.steps = this.ToShunting(this.expression);
    #endif
        if (this.steps == null || this.steps.Count == 0)
        {
            return 0.0f;
        }
        List<float> inputs = new();
        foreach (RDRSReaderBase reader in sources)
        {
            object raw = reader?.GetValue();
            if (raw is bool b)
            {
                inputs.Add(b ? 1.0f : -0.0f);
            }
            else
            {
                try
                {
                    float f = System.Convert.ToSingle(raw);
                    inputs.Add(f);
                }
                catch
                {
                    Debug.LogWarning($"[MathMiddleware] waiting for only floats but got: {raw?.GetType().Name ?? "null"}");
                    return 0f;
                }
            }
        }
        return this.EvaluateShuntingRPN(this.steps, inputs);
    }

    private float EvaluateShuntingRPN(List<string> rpnTokens, List<float> inputs)
    {
        Stack<float> stack = new();

        foreach (string token in rpnTokens)
        {
            if (float.TryParse(token, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float number))
            {
                stack.Push(number);
            }
            else if (this.IsVariableToken(token))
            {
                string cleanToken = token.Trim('|');
                int index = this.ParseVariableIndex(cleanToken);
                float value = (index >= 0 && index < inputs.Count) ? inputs[index] : 0f;
                
                if(cleanToken.Length != token.Length)
                {
                    value = MathF.Abs(value);
                }
                
                stack.Push(value);
            }
            else if (IsOperator(token))
            {
                float b = stack.Pop();
                float a = stack.Pop();
                float result;
                switch (token)
                {
                    case "+":
                        result = a + b;
                        break;
                    case "-":
                        result = a - b;
                        break;
                    case "*":
                        result = a * b;
                        break;
                    case "/":
                        result = a / b;
                        break;
                    case ">":
                        result = a > b ? 1f : 0f;
                        break;
                    case "<":
                        result = a < b ? 1f : 0f;
                        break;
                    case "==":
                        result = Mathf.Approximately(a, b) ? 1f : 0f;
                        break;
                    case "!=":
                        result = !Mathf.Approximately(a, b) ? 1f : 0f;
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported operator: {token}");
                }
                stack.Push(result);
            }
            else
            {
                throw new InvalidOperationException($"Unexpected token in RPN expression: {token}");
            }
        }

        return stack.Count == 1 ? stack.Pop() : throw new InvalidOperationException("Invalid RPN expression evaluation.");
    }


    /* Prepare */
    /** I will not lie...i more or less undestand what this code do */
    private List<string> ToShunting(string expression)
    {
        List<string> output = new List<string>();
        Stack<string> simbolsCollection = new Stack<string>();
        string[] tokens = expression.Split(' ');

        foreach (string token in tokens)
        {
            if (float.TryParse(token, out _) || this.IsVariableToken(token))
            {
                output.Add(token);
            }
            else if (IsOperator(token))
            {
                while (simbolsCollection.Count > 0 && IsOperator(simbolsCollection.Peek()) && GetPrecedence(token) <= GetPrecedence(simbolsCollection.Peek()))
                {
                    output.Add(simbolsCollection.Pop());
                }
                simbolsCollection.Push(token);
            }
            else if (token == "(")
            {
                simbolsCollection.Push(token);
            }
            else if (token == ")")
            {
                while (simbolsCollection.Count > 0 && simbolsCollection.Peek() != "(")
                {
                    output.Add(simbolsCollection.Pop());
                }
                if (simbolsCollection.Count == 0 || simbolsCollection.Pop() != "(")
                {
                    throw new ArgumentException("Mismatched parentheses in expression");
                }
            }
        }

        while (simbolsCollection.Count > 0)
        {
            string t = simbolsCollection.Pop();
            if (t == "(" || t == ")")
            {
                throw new ArgumentException("Mismatched parentheses in expression");
            }
            output.Add(t);
        }
        return output;
    }

    private int GetPrecedence(string op)
    {
        switch (op)
        {
            case "+":
            case "-":
                return 1;
            case "*":
            case "/":
                return 2;
            case ">":
            case "<":
            case "==":
            case "!=":
                return 0;
            default:
                return -1;
        }
    }

    private bool IsOperator(string token)
    {
        switch (token)
        {
            case "+":
            case "-":
            case "*":
            case "/":
            case ">":
            case "<":
            case "==":
            case "!=":
                return true;
            default:
                return false;
        }
    }

    
    private bool IsVariableToken(string token)
    {
        return Regex.IsMatch(token, @"^(\|\{\d+\}\||\{\d+\})$");
    }
    
    private int ParseVariableIndex(string token)
    {
        return int.Parse(token.Substring(1, token.Length - 2));
    }
}
