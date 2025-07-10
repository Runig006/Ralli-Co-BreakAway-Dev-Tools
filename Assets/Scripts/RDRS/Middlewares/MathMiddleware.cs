using UnityEngine;
using System.Collections.Generic;
using System;

public enum MathOp
{
    Add,
    Subtract,
    Multiply,
    Divide
}

[System.Serializable]
public class OperationStep
{
    public MathOp operation;
    public string valueId;

    public float GetEvaluatedValue(List<float> inputs)
    {
        if (valueId.StartsWith("$"))
        {
            return inputs[int.Parse(valueId.Substring(1)) - 1];
        }
        return float.Parse(valueId);
    }
}


public class MathMiddleware : RDRSReaderBase<float>
{
    [SerializeField] private List<RDRSReaderBase<float>> sources = new();
    [SerializeField] private string expression = "$1 * 0.5 + $2";

    private List<OperationStep> steps;

    private void Awake()
    {
        steps = this.CompileOperation(expression);
    }

    public override float GetValue()
    {
        List<float> inputs = new();
        foreach (var reader in sources)
        {
            inputs.Add(reader?.GetValue() ?? 0f);
        }

        float total = 0f;
        foreach (OperationStep step in steps)
        {
            float value = step.GetEvaluatedValue(inputs);

            switch (step.operation)
            {
                case MathOp.Add:
                    total += value;
                    break;
                case MathOp.Subtract:
                    total -= value;
                    break;
                case MathOp.Multiply:
                    total *= value;
                    break;
                case MathOp.Divide:
                    total /= value;
                    break;
            }
        }
        return total;
    }

    private List<OperationStep> CompileOperation(string expression)
    {
        List<OperationStep> steps = new List<OperationStep>();
        String[] tokens = expression.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            throw new ArgumentException("Operation not valid");
        }
        MathOp currentOp = MathOp.Add;
        MathOp? tempOp;
        foreach (String token in tokens)
        {
            tempOp = this.TranslateToOperation(token);
            //Is the operation
            if (tempOp != null)
            {
                currentOp = tempOp ?? MathOp.Add;
            }
            else
            {
                steps.Add(new OperationStep
                {
                    operation = currentOp,
                    valueId = token
                });
            }
        }
        return steps;
    }

    private MathOp? TranslateToOperation(string s)
    {
        switch (s)
        {
            case "+":
                return MathOp.Add;
            case "-":
                return MathOp.Subtract;
            case "*":
                return MathOp.Multiply;
            case "/":
                return MathOp.Divide;
            default:
                return null;
        }
        ;
    }
}
