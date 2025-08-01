using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class MathMiddleware : RDRSNode
{
    [SerializeField] private RDRSNode[] sources;
    [SerializeField] private string expression = "{0} * 0.5 + {1}";

    private List<string> steps;
    private float result;

    private void Awake()
    {
        this.steps = this.ToShunting(this.expression);
    }

    private void OnValidate()
    {
        this.steps = this.ToShunting(this.expression);
    }

    public override object GetValue()
    {
        if (this.steps == null || this.steps.Count == 0)
        {
            return 0.0f;
        }
        List<float> inputs = new();
        foreach (RDRSNode reader in sources)
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
        return result = this.EvaluateShuntingRPN(this.steps, inputs);
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
                int index = int.Parse(this.RemoveTokensFromIndex(token));
                float value = (index >= 0 && index < inputs.Count) ? inputs[index] : 0f;

                if (cleanToken.Length != token.Length)
                {
                    value = MathF.Abs(value);
                }

                stack.Push(value);
            }
            else if (this.IsOperator(token))
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
                    case ">=":
                        result = a >= b ? 1f : 0f;
                        break;
                    case "<=":
                        result = a <= b ? 1f : 0f;
                        break;
                    case "==":
                        result = Mathf.Approximately(a, b) ? 1f : 0f;
                        break;
                    case "!=":
                        result = !Mathf.Approximately(a, b) ? 1f : 0f;
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported binary operator: {token}");
                }
                stack.Push(result);
            }
            else if (this.IsFunction(token))
            {
                float a = stack.Pop();
                float result;
                switch (token)
                {
                    case "sin":
                        result = Mathf.Sin(a);
                        break;
                    case "cos":
                        result = Mathf.Cos(a);
                        break;
                    case "tan":
                        result = Mathf.Tan(a);
                        break;
                    case "sqrt":
                        result = Mathf.Sqrt(a);
                        break;
                    case "floor":
                        result = Mathf.Floor(a);
                        break;
                    case "ceil":
                        result = Mathf.Ceil(a);
                        break;
                    case "round":
                        result = Mathf.Round(a);
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported function: {token}");
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
            else if (this.IsFunction(token) || this.IsOperator(token))
            {
                while (
                    simbolsCollection.Count > 0 &&
                    (this.IsOperator(simbolsCollection.Peek()) || this.IsFunction(simbolsCollection.Peek())) &&
                    this.GetPrecedence(token) <= this.GetPrecedence(simbolsCollection.Peek())
                )
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
            case "sin":
            case "cos":
            case "tan":
            case "sqrt":
            case "floor":
            case "ceil":
            case "round":
                return 3;
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
            case ">=":
            case "<":
            case "<=":
            case "==":
            case "!=":
                return true;
            default:
                return false;
        }
    }

    private bool IsFunction(string token)
    {
        switch (token)
        {
            case "sin":
            case "cos":
            case "tan":
            case "sqrt":
            case "floor":
            case "ceil":
            case "round":
                return true;
            default:
                return false;
        }
    }

    private bool IsVariableToken(string token)
    {
        //Regex is slow
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }
        string inner = this.RemoveTokensFromIndex(token);
        if (inner.Length == token.Length)
        {
            return false;
        }

        for (int i = 0; i < inner.Length; i++)
        {
            if (char.IsDigit(inner[i]) == false)
            {
                return false;
            }
        }

        return true;
    }

    private string RemoveTokensFromIndex(string token)
    {
        return token.Replace("|", "").Replace("{", "").Replace("}", "");
    }
}
