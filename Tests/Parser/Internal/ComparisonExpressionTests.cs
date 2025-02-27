﻿using Jamiras.Components;
using NUnit.Framework;
using RATools.Parser;
using RATools.Parser.Functions;
using RATools.Parser.Internal;
using System.Text;

namespace RATools.Test.Parser.Internal
{
    [TestFixture]
    class ComparisonExpressionTests
    {
        [Test]
        [TestCase(ComparisonOperation.Equal, "variable == 99")]
        [TestCase(ComparisonOperation.NotEqual, "variable != 99")]
        [TestCase(ComparisonOperation.LessThan, "variable < 99")]
        [TestCase(ComparisonOperation.LessThanOrEqual, "variable <= 99")]
        [TestCase(ComparisonOperation.GreaterThan, "variable > 99")]
        [TestCase(ComparisonOperation.GreaterThanOrEqual, "variable >= 99")]
        public void TestAppendString(ComparisonOperation op, string expected)
        {
            var variable = new VariableExpression("variable");
            var value = new IntegerConstantExpression(99);
            var expr = new ComparisonExpression(variable, op, value);

            var builder = new StringBuilder();
            expr.AppendString(builder);
            Assert.That(builder.ToString(), Is.EqualTo(expected));
        }

        [Test]
        [TestCase("byte(1) > variable2", "byte(1) > 99")] // simple variable substitution
        [TestCase("variable1 > variable2", "false")] // evaluates to a constant
        [TestCase("1 == byte(2)", "byte(2) == 1")] // move constant to right side
        [TestCase("1 != byte(2)", "byte(2) != 1")] // move constant to right side
        [TestCase("1 < byte(2)", "byte(2) > 1")] // move constant to right side
        [TestCase("1 <= byte(2)", "byte(2) >= 1")] // move constant to right side
        [TestCase("1 > byte(2)", "byte(2) < 1")] // move constant to right side
        [TestCase("1 >= byte(2)", "byte(2) <= 1")] // move constant to right side
        [TestCase("byte(1) == 3.14", "Result can never be true using integer math")] // cannot compare byte() to float
        [TestCase("byte(1) != 3.14", "Result is always true using integer math")] // cannot compare byte() to float
        [TestCase("byte(1) < 3.14", "byte(1) <= 3")] // adjust to integer math
        [TestCase("byte(1) <= 3.14", "byte(1) <= 3")] // adjust to integer math
        [TestCase("byte(1) > 3.14", "byte(1) > 3")] // adjust to integer math
        [TestCase("byte(1) >= 3.14", "byte(1) > 3")] // adjust to integer math
        [TestCase("float(1) == 3.14", "float(1) == 3.14")] // cannot compare byte() to float
        [TestCase("byte(1) + 1 < byte(2) + 1", "byte(1) < byte(2)")] // same modifier on both sides can be eliminated
        [TestCase("byte(1) * 2 + 1 == byte(2) * 2 + 1", "byte(1) == byte(2)")] // same modifiers on both sides can be eliminated
        [TestCase("byte(1) + 6 < byte(2) + 3", "byte(1) + 3 < byte(2)")] // differing modifier should be merged
        [TestCase("byte(1) + variable1 < byte(2) + 3", "byte(1) + 95 < byte(2)")] // differing modifier should be merged
        [TestCase("byte(1) - 1 == 4", "byte(1) == 5")] // factor out subtraction
        [TestCase("byte(1) + 1 == 4", "byte(1) == 3")] // factor out addition
        [TestCase("byte(1) + 1.2 == 4.8", "Result can never be true using integer math")] // will convert to "byte(1) == 3.6", which cannot be true
        [TestCase("byte(1) * 10 == 100", "byte(1) == 10")] // factor out multiplication
        [TestCase("byte(1) * 10 == 99", "Result can never be true using integer math")] // multiplication cannot be factored out
        [TestCase("byte(1) * 10 == 99.0", "Result can never be true using integer math")] // multiplication cannot be factored out
        [TestCase("byte(1) * 10.0 == 99", "Result can never be true using integer math")] // multiplication cannot be factored out
        [TestCase("byte(1) * 10.0 == 99.0", "Result can never be true using integer math")] // multiplication cannot be factored out
        [TestCase("float(1) * 10 == 99", "float(1) == 9.9")] // factor out multiplication
        [TestCase("float(1) * 2.2 == 7.4", "float(1) == 3.363636")] // factor out multiplication
        [TestCase("byte(1) * 2.2 == 6.6", "byte(1) == 3")] // factor out multiplication
        [TestCase("byte(1) * 10 != 100", "byte(1) != 10")] // factor out multiplication
        [TestCase("byte(1) * 10 != 99", "Result is always true using integer math")] // multiplication cannot be factored out
        [TestCase("byte(1) * 10 < 99", "byte(1) <= 9")] // factor out multiplication - become less than or equal
        [TestCase("byte(1) * 10 < 90", "byte(1) < 9")] // factor out multiplication - does not become less than or equal
        [TestCase("byte(1) * 10 <= 99", "byte(1) <= 9")] // factor out multiplication
        [TestCase("byte(1) * 10 > 99", "byte(1) > 9")] // factor out multiplication
        [TestCase("byte(1) * 10 >= 99", "byte(1) > 9")] // factor out multiplication - becomes greater than
        [TestCase("byte(1) * 10 >= 90", "byte(1) >= 9")] // factor out multiplication - does not become greater than
        [TestCase("byte(1) / 10 == 4", "byte(1) == 40")] // factor out division
        [TestCase("byte(1) / 10 < 9", "byte(1) < 90")] // factor out division
        [TestCase("byte(1) * 10 * 2 == 100", "byte(1) == 5")] // factor out multiplication
        [TestCase("2 * byte(1) * 10 == 100", "byte(1) == 5")] // factor out multiplication
        [TestCase("2.2 * byte(1) * 10 == 100", "Result can never be true using integer math")] // factor out multiplication
        [TestCase("2.2 * float(1) * 10 == 100", "float(1) == 4.545455")] // factor out multiplication
        [TestCase("byte(1) * 10 / 2 == 100", "byte(1) == 20")] // factor out multiplication and division
        [TestCase("byte(1) * 10 / 3 == 100", "byte(1) == 30")] // factor out multiplication and division
        [TestCase("byte(1) * 10 + 10 == 100", "byte(1) == 9")] // factor out multiplication and addition
        [TestCase("byte(1) * 10 - 10 == 100", "byte(1) == 11")] // factor out multiplication and subtraction
        [TestCase("(byte(1) - 1) * 10 == 100", "byte(1) == 11")] // factor out multiplication and subtraction
        [TestCase("(byte(1) - 1) / 10 == 10", "byte(1) == 101")] // factor out division and subtraction
        [TestCase("(byte(1) - 1) * 10 < 99", "byte(1) <= 10")] // factor out division and subtraction
        [TestCase("byte(1) * 10 + byte(2) == 100", "byte(1) * 10 + byte(2) == 100")] // multiplication cannot be factored out
        [TestCase("byte(1) * 10 == byte(2)", "byte(1) * 10 == byte(2)")] // multiplication cannot be factored out
        [TestCase("byte(2) + 1 == variable1", "byte(2) == 97")] // differing modifier should be merged
        [TestCase("variable1 == byte(2) + 1", "byte(2) == 97")] // differing modifier should be merged, move constant to right side
        [TestCase("byte(1) + 3 == prev(byte(1))", "byte(1) + 3 == prev(byte(1))")] // value decreases by 3
        [TestCase("byte(1) == prev(byte(1)) - 3", "byte(1) + 3 == prev(byte(1))")] // value decreases by 3
        [TestCase("prev(byte(1)) - byte(1) == 3", "prev(byte(1)) - byte(1) == 3")] // value decreases by 3
        [TestCase("byte(1) - 3 == prev(byte(1))", "byte(1) - 3 == prev(byte(1))")] // value increases by 3
        [TestCase("byte(1) == prev(byte(1)) + 3", "byte(1) - 3 == prev(byte(1))")] // value increases by 3
        [TestCase("byte(1) - prev(byte(1)) == 3", "byte(1) - prev(byte(1)) == 3")] // value increases by 3
        [TestCase("0 + byte(1) + 0 == 9", "byte(1) == 9")] // 0s should be removed without reordering
        [TestCase("0 + byte(1) - 9 == 0", "byte(1) == 9")] // 9 should be moved to right hand side, then 0s removed
        [TestCase("bcd(byte(1)) == 24", "byte(1) == 36")] // bcd should be factored out
        [TestCase("byte(1) != bcd(byte(2))", "byte(1) != bcd(byte(2))")] // bcd cannot be factored out
        [TestCase("bcd(byte(1)) != prev(bcd(byte(1)))", "byte(1) != prev(byte(1))")] // bcd should be factored out
        [TestCase("byte(1) / byte(2) < 0.8", "byte(1) / 0.8 < byte(2)")] // move float to avoid integer division
        [TestCase("byte(1) * 100.0 / byte(2) > 75", "byte(1) * 100.0 / byte(2) > 75")] // division could not be moved
        [TestCase("byte(1) / byte(2) * 100.0 > 75", "byte(1) / 0.75 > byte(2)")] // combine numbers, then move float to avoid integer division
        public void TestReplaceVariables(string input, string expected)
        {
            var tokenizer = Tokenizer.CreateTokenizer(input);
            var expr = ExpressionBase.Parse(new PositionalTokenizer(tokenizer));

            var scope = new InterpreterScope(RATools.Parser.AchievementScriptInterpreter.GetGlobalScope());
            scope.Context = new RATools.Parser.TriggerBuilderContext();
            scope.AssignVariable(new VariableExpression("variable1"), new IntegerConstantExpression(98));
            scope.AssignVariable(new VariableExpression("variable2"), new IntegerConstantExpression(99));

            ExpressionBase result;
            if (!expr.ReplaceVariables(scope, out result))
                Assert.That(result, Is.InstanceOf<ParseErrorExpression>());

            var builder = new StringBuilder();
            result.AppendString(builder);
            Assert.That(builder.ToString(), Is.EqualTo(expected));
        }

        // If the result of subtracting two bytes is negative, it becomes a very large positive
        // number so you can't perform less than checks. Try to rearrange the logic so no subtraction
        // is performed. If that's not possible, add a constant to both sides of the equation to
        // prevent the subtraction from resulting in a negative number.
        [Test]
        [TestCase("A < B", "A < B")] // control
        [TestCase("A - 10 < B", "B + 10 > A")] // reverse and change to addition
        [TestCase("A - 10 <= B", "B + 10 >= A")] // reverse and change to addition
        [TestCase("A - 10 > B", "B + 10 < A")] // reverse and change to addition
        [TestCase("A - 10 >= B", "B + 10 <= A")] // reverse and change to addition
        [TestCase("A - 10 == B", "A - 10 == B")] // no change needed for equality
        [TestCase("A - 10 != B", "A - 10 != B")] // no change needed for inequality
        [TestCase("A + 10 < B", "A + 10 < B")] // no change needed for addition
        [TestCase("A + 10 > B", "A + 10 > B")] // no change needed for addition
        [TestCase("A + 10 == B", "A + 10 == B")] // no change needed for addition or equality
        [TestCase("A + 10 != B", "A + 10 != B")] // no change needed for addition or inequality
        [TestCase("A > B - 10", "A + 10 > B")] // move -10 to left
        [TestCase("A > B + 10", "B + 10 < A")] // swap order so addition is on left
        [TestCase("A == B + 10", "A - 10 == B")] // move +10 to left, no further change for equality
        [TestCase("A + B > 10", "A + B > 10")] // no change needed for addition
        [TestCase("A - B > 10", "B + 10 < A")] // reverse and change to addition
        [TestCase("A - B < 3", "A - B + 255 < 258")] // reverse and change to addition
        [TestCase("A - B > -3", "A + 3 > B")] // move -3 to left side and B to right side
        [TestCase("A - B == 10", "A - B == 10")] // don't rearrange equality comparisons
        [TestCase("A - B != 10", "A - B != 10")] // don't rearrange equality comparisons
        [TestCase("A + 1 - B > 3", "A - B + 255 > 257")] // explicit underflow adjustment ignored for greater than
        [TestCase("A + 1 - B <= 2", "A - B + 1 <= 2")] // explicit underflow adjustment provided
        [TestCase("A + 3 - B > 1", "A - B + 255 > 253")] // explicit underflow adjustment ignored for greater than
        [TestCase("A + 3 - B == 1", "A + 2 == B")] // explicit underflow adjustment ignored for equality, move B right and 1 left
        [TestCase("A + 1 - B > -3", "A - B + 255 > 251")] // explicit underflow ignored for greater than
        [TestCase("A + 1 - B < -3", "A - B + 255 < 251")] // explicit underflow ignored when right side is negative
        [TestCase("A - B + 355 > 255", "A + 100 > B")] // move B to right side, and 100 to left side 
        [TestCase("5 - A < 2", "A > 3")] // move A to right side, 2 to left, and reverse
        [TestCase("5 - A == 2", "A == 3")] // move A to right side, 2 to left, and reverse
        [TestCase("300 - A < 100", "A > 200")] // move A to right side, 100 to left, and reverse
        [TestCase("A + B + C < 100", "A + B + C < 100")] // no change needed
        [TestCase("A + B - C < 100", "A + B - C + 255 < 355")] // possible underflow of 255, add to both sides
        [TestCase("A - B + C < 100", "A - B + C + 255 < 355")] // possible underflow of 255, add to both sides
        [TestCase("A - B - C < 100", "A - B - C + 510 < 610")] // possible underflow of 510, add to both sides
        [TestCase("A - B - C < -100", "A - B - C + 510 < 410")] // possible underflow of 510, add to both sides
        [TestCase("A - B - C + 700 < 800", "A - B - C + 510 < 610")] // excess underflow coverage will be minimized
        [TestCase("A - B - C + 300 < 100", "A - B - C + 510 < 310")] // possible underflow of 210, add to both sides
        [TestCase("A - B < C + 100", "A - B - C + 510 < 610")] // move C to left side, possible underflow of 510
        [TestCase("A - 100 > B - C", "A - (B - C) + 255 > 355")] // move 100 to right side, B and C to left, and add 255 to prevent underflow
        [TestCase("A * 2 - B < 3", "A * 2 - B + 255 < 258")] // reverse and change to addition
        [TestCase("A - B * 2 < 3", "A - (B * 2) + 510 < 513")] // reverse and change to addition
        [TestCase("A / 2 - B < 3", "A / 2 - B + 255 < 258")] // reverse and change to addition
        [TestCase("A - B / 2 < 3", "A - (B / 2) + 127 < 130")] // reverse and change to addition
        [TestCase("A / A - (B / B) >= 1", "B / B + 1 <= A / A")]
        [TestCase("A / B - (B / C) >= 1", "B / C + 1 <= A / B")]
        [TestCase("A / A - 2 > (B / B)", "A / A - (B / B) + 1 > 3")] // B/B resolves to 1
        [TestCase("A / B - 2 > (B / C)", "A / B - (B / C) + 255 > 257")] // B/C resolves to 255
        public void TestUnderflow(string input, string expected)
        {
            var comparison = input.Replace("A", "byte(1)").Replace("B", "byte(2)").Replace("C", "byte(3)");
            var expectedComparison = expected.Replace("A", "byte(1)").Replace("B", "byte(2)").Replace("C", "byte(3)");

            // SubSource(mem) can cause wraparound, so if modifiers are present when doing a
            // less than comparison, assume they're there to prevent the wraparound and don't
            // transfer them to the right side.
            var tokenizer = Tokenizer.CreateTokenizer(comparison);
            var expr = ExpressionBase.Parse(new PositionalTokenizer(tokenizer));

            var scope = new InterpreterScope();
            scope.Context = new RATools.Parser.TriggerBuilderContext();
            scope.AddFunction(new MemoryAccessorFunction("byte", RATools.Data.FieldSize.Byte));

            ExpressionBase result;
            if (!expr.ReplaceVariables(scope, out result))
                Assert.That(result, Is.InstanceOf<ParseErrorExpression>());

            var builder = new StringBuilder();
            result.AppendString(builder);
            Assert.That(builder.ToString(), Is.EqualTo(expectedComparison));

            var minimum = input.Contains("/") ? "1" : "0";

            // prove that the logic is equivalent                  0123456789
            // ignore items where the explicit underflow was kept (A + 1 - B  ~> A - B + 1)
            var swapped = (input.Length > 9) ? input.Substring(0, 1) + input.Substring(5, 4) + input.Substring(1, 4) + input.Substring(9) : string.Empty;
            if (swapped != expected)
            {
                var values = new string[] { minimum, "10", "100", "255" };
                foreach (var a in values)
                {
                    var bValues = input.Contains("B") ? values : new string[] { minimum };
                    foreach (var b in bValues)
                    {
                        var cValues = input.Contains("C") ? values : new string[] { minimum };
                        foreach (var c in cValues)
                        {
                            var original = input.Replace("A", a).Replace("B", b).Replace("C", c);
                            tokenizer = Tokenizer.CreateTokenizer(original);
                            var originalExpression = ExpressionBase.Parse(new PositionalTokenizer(tokenizer));
                            var originalEval = IsComparisonTrueSigned(originalExpression);

                            var updated = expected.Replace("A", a).Replace("B", b).Replace("C", c);
                            tokenizer = Tokenizer.CreateTokenizer(updated);
                            var updatedExpression = ExpressionBase.Parse(new PositionalTokenizer(tokenizer));
                            var updatedEval = IsComparisonTrueUnsigned(updatedExpression);

                            Assert.That(originalEval, Is.EqualTo(updatedEval), "{0} ({1})  ~>  {2} ({3})", original, originalEval, updated, updatedEval);
                        }
                    }
                }
            }
        }

        private static bool IsComparisonTrueSigned(ExpressionBase expression)
        {
            Assert.That(expression, Is.InstanceOf<ComparisonExpression>());
            var comparison = (ComparisonExpression)expression;

            // the two sides of comparison should be mathematic equations that simplify to single integers
            ExpressionBase left;
            comparison.Left.ReplaceVariables(null, out left);
            Assert.That(left, Is.InstanceOf<IntegerConstantExpression>());
            int lval = ((IntegerConstantExpression)left).Value;

            ExpressionBase right;
            comparison.Right.ReplaceVariables(null, out right);
            Assert.That(right, Is.InstanceOf<IntegerConstantExpression>());
            int rval = ((IntegerConstantExpression)right).Value;

            switch (comparison.Operation)
            {
                case ComparisonOperation.Equal:
                    return lval == rval;
                case ComparisonOperation.NotEqual:
                    return lval != rval;
                case ComparisonOperation.GreaterThan:
                    return lval > rval;
                case ComparisonOperation.GreaterThanOrEqual:
                    return lval >= rval;
                case ComparisonOperation.LessThan:
                    return lval < rval;
                case ComparisonOperation.LessThanOrEqual:
                    return lval <= rval;
                default:
                    return false;
            }
        }

        private static bool IsComparisonTrueUnsigned(ExpressionBase expression)
        {
            Assert.That(expression, Is.InstanceOf<ComparisonExpression>());
            var comparison = (ComparisonExpression)expression;

            // the two sides of comparison should be mathematic equations that simplify to single integers
            ExpressionBase left;
            comparison.Left.ReplaceVariables(null, out left);
            Assert.That(left, Is.InstanceOf<IntegerConstantExpression>());
            uint lval = (uint)((IntegerConstantExpression)left).Value;

            ExpressionBase right;
            comparison.Right.ReplaceVariables(null, out right);
            Assert.That(right, Is.InstanceOf<IntegerConstantExpression>());
            uint rval = (uint)((IntegerConstantExpression)right).Value;

            switch (comparison.Operation)
            {
                case ComparisonOperation.Equal:
                    return lval == rval;
                case ComparisonOperation.NotEqual:
                    return lval != rval;
                case ComparisonOperation.GreaterThan:
                    return lval > rval;
                case ComparisonOperation.GreaterThanOrEqual:
                    return lval >= rval;
                case ComparisonOperation.LessThan:
                    return lval < rval;
                case ComparisonOperation.LessThanOrEqual:
                    return lval <= rval;
                default:
                    return false;
            }
        }

        // TestUnderflow only supports byte(N), it does not support word(N), dword(N), or indirect
        // memory references. Furthermore, the way it handles indirect memory references creates
        // invalid syntax (indirect memory references are not allowed on the right side), so go
        // one step farther to see the final optimized logic.
        [TestCase("byte(byte(2) + 1) - byte(byte(2) + 2) > 100",
                  "byte(byte(2) + 2) + 100 < byte(byte(2) + 1)", // A - B > 100  ~>  B + 100 < A
                  "A:100_I:0xH000002_0xH000002<0xH000001")]      // both A and B have the same base pointer
        [TestCase("byte(byte(2) + 1) - byte(byte(2) + 2) > -100",
                  "byte(byte(2) + 1) + 100 > byte(byte(2) + 2)", // A - B > -100  ~>  A + 100 > B
                  "A:100_I:0xH000002_0xH000001>0xH000002")]      // both A and B have the same base pointer
        [TestCase("byte(byte(2) + 1) - byte(byte(3) + 2) > 100",
                  "byte(byte(3) + 2) + 100 < byte(byte(2) + 1)", // A - B > 100  ~>  B + 100 < A
                  "I:0xH000003_A:0xH000002_I:0xH000002_B:0xH000001_100>255")] // different base pointer causes secondary AddSource
        [TestCase("word(54) - word(word(43102) + 54) > 37",
                  "word(word(43102) + 54) + 37 < word(54)", // A - B > 37  ~>  B + 37 < A
                  "A:37_I:0x 00a85e_A:0x 000036_0<0x 000036")] // underflow with combination of direct/indirect, word size
        [TestCase("word(54) + 37 >= word(word(43102) + 54)",
                  "word(54) + 37 >= word(word(43102) + 54)", // A + N >= B  ~>  A + N >= B
                  "A:0x 000036_I:0x 00a85e_B:0x 000036_65572>=65535")] // combination of direct/indirect, word size
        [TestCase("word(1) - word(2) + word(3) < 100",
                  "word(1) - word(2) + word(3) + 65535 < 65635", // possible underflow of 65535
                  "A:65535=0_B:0x 000002=0_A:0x 000001=0_0x 000003<65635")]
        [TestCase("dword(1) - dword(2) + dword(3) < 100",
                  "dword(1) - dword(2) + dword(3) < 100", // possible underflow of 2^32-1, ignore
                  "B:0xX000002=0_A:0xX000001=0_0xX000003<100")]
        [TestCase("byte(dword(1)) - byte(dword(2)) + byte(dword(3)) < 100",
                  "byte(dword(1)) - byte(dword(2)) + byte(dword(3)) + 255 < 355", // reads are only bytes, underflow is 255
                  "A:255_I:0xX000002_B:0xH000000_I:0xX000001_A:0xH000000_I:0xX000003_0xH000000<355")]
        [TestCase("word(1) - word(2) - byte(3) < 100",
                  "word(1) - word(2) - byte(3) + 65790 < 65890", // combination of byte and word
                  "A:65790=0_B:0xH000003=0_B:0x 000002=0_0x 000001<65890")]
        [TestCase("byte(1) + byte(2) > byte(3) - byte(4)",
                  "byte(1) + byte(2) + byte(4) > byte(3)", // move byte(4) to left side
                  "A:0xH000001=0_A:0xH000002=0_0xH000004>0xH000003")]
        [TestCase("byte(1) + byte(2) > byte(3) + byte(4)",
                  "byte(1) + byte(2) - byte(3) - byte(4) + 510 > 510", // move byte(3) to left side, add 255 to prevent underflow
                  "A:510=0_B:0xH000004=0_B:0xH000003=0_A:0xH000001=0_0xH000002>510")]
        [TestCase("bit1(1) + bit2(1) > bit3(1) + bit4(1)",
                  "bit1(1) + bit2(1) - bit3(1) - bit4(1) + 2 > 2", // underflow of 2 calculated
                  "A:2=0_B:0xQ000001=0_B:0xP000001=0_A:0xN000001=0_0xO000001>2")]
        [TestCase("bit1(1) + bit2(1) > bit3(1) - bit4(1)",
                  "bit1(1) + bit2(1) + bit4(1) > bit3(1)", // rearrange so single field on right
                  "A:0xN000001=0_A:0xO000001=0_0xQ000001>0xP000001")]
        [TestCase("bit1(1) + bit2(1) > bit3(1) + bit4(1) + 1",
                  "bit1(1) + bit2(1) - (bit3(1) + bit4(1)) + 2 > 3", // underflow of 2 calculated
                  "A:2=0_B:0xP000001=0_B:0xQ000001=0_A:0xN000001=0_0xO000001>3")]
        [TestCase("bit1(1) + bit2(1) < bit3(1) + bit4(1) + 1",
                  "bit1(1) + bit2(1) - (bit3(1) + bit4(1)) + 2 < 3", // underflow of 2 calculated
                  "A:2=0_B:0xP000001=0_B:0xQ000001=0_A:0xN000001=0_0xO000001<3")]
        [TestCase("bit1(1) + bit2(1) + 3 > bit3(1) + bit4(1) + 5",
                  "bit1(1) + bit2(1) - (bit3(1) + bit4(1)) + 2 > 4", // constants merged, then underflow of 2 applied
                  "A:2=0_B:0xP000001=0_B:0xQ000001=0_A:0xN000001=0_0xO000001>4")]
        [TestCase("bit1(1) + bit2(1) - bit3(1) - bit4(1) < 1",
                  "bit1(1) + bit2(1) - bit3(1) - bit4(1) + 2 < 3", // underflow of 2 calculated
                  "A:2=0_B:0xQ000001=0_B:0xP000001=0_A:0xN000001=0_0xO000001<3")]
        [TestCase("bit1(1) + bit2(1) + 2 - bit3(1) - bit4(1) < 3",
                  "bit1(1) + bit2(1) - bit3(1) - bit4(1) + 2 < 3", // underflow of 2 calculated
                  "A:2=0_B:0xQ000001=0_B:0xP000001=0_A:0xN000001=0_0xO000001<3")]
        [TestCase("byte(1) + 1 - byte(2) >= 2",
                  "byte(1) - byte(2) + 255 >= 256", // 254 added to both sides to prevent underflow
                  "A:255=0_B:0xH000002=0_0xH000001>=256")]
        [TestCase("byte(1) + 1 - byte(2) < 2",
                  "byte(1) - byte(2) + 1 < 2", // user-provided underflow adjustment kept
                  "A:1=0_B:0xH000002=0_0xH000001<2")]
        [TestCase("byte(1) - prev(byte(1)) >= 2",
                  "prev(byte(1)) + 2 <= byte(1)", // rearrange to avoid subtraction
                  "A:2=0_d0xH000001<=0xH000001")]
        [TestCase("prev(byte(1)) - prev(byte(2)) - prev(byte(3)) < 2", // make sure the memory reference is seen inside the prev
                  "prev(byte(1)) - prev(byte(2)) - prev(byte(3)) + 510 < 512", // overflow of 510 calculated
                  "A:510=0_B:d0xH000003=0_B:d0xH000002=0_d0xH000001<512")]
        [TestCase("(word(1) - word(2)) > 0 && (word(1) - word(2)) < 0x8000",
                  "word(1) - word(2) + 65535 > 65535 && word(1) - word(2) + 65535 < 98303",
                  "A:65535=0_B:0x 000002=0_0x 000001>65535_A:65535=0_B:0x 000002=0_0x 000001<98303")]
        public void TestUnderflowComplex(string input, string expected, string expectedSerialized)
        {
            var tokenizer = Tokenizer.CreateTokenizer(input);
            var expr = ExpressionBase.Parse(new PositionalTokenizer(tokenizer));

            var scope = new InterpreterScope(RATools.Parser.AchievementScriptInterpreter.GetGlobalScope());
            scope.Context = new RATools.Parser.TriggerBuilderContext();

            ExpressionBase result;
            if (!expr.ReplaceVariables(scope, out result))
                Assert.That(result, Is.InstanceOf<ParseErrorExpression>());

            var builder = new StringBuilder();
            result.AppendString(builder);
            Assert.That(builder.ToString(), Is.EqualTo(expected));

            var achievementBuilder = new RATools.Parser.ScriptInterpreterAchievementBuilder();
            achievementBuilder.PopulateFromExpression(result);
            var serialized = achievementBuilder.SerializeRequirements();
            Assert.That(serialized, Is.EqualTo(expectedSerialized));
        }

        [Test]
        public void TestUnderflowAdjustmentImpossible()
        {
            var input = "5 + byte(0x1234) == 2";
            var tokenizer = Tokenizer.CreateTokenizer(input);
            var expr = ExpressionBase.Parse(new PositionalTokenizer(tokenizer));

            var scope = new InterpreterScope();
            scope.Context = new RATools.Parser.TriggerBuilderContext();
            scope.AddFunction(new MemoryAccessorFunction("byte", RATools.Data.FieldSize.Byte));

            ExpressionBase result;
            Assert.That(expr.ReplaceVariables(scope, out result), Is.False);
            Assert.That(result, Is.InstanceOf<ParseErrorExpression>());
            Assert.That(((ParseErrorExpression)result).Message, Is.EqualTo("Expression can never be true"));
        }

        [Test]
        [TestCase("0 == 0", true)]
        [TestCase("0 == 1", false)]
        [TestCase("byte(0) == 0", null)]
        [TestCase("0 == 0.0", true)]
        [TestCase("0 == 1.0", false)]
        [TestCase("0.0 == 0.0", true)]
        [TestCase("0.0 == 1.0", false)]
        [TestCase("0.0 == 0", true)]
        [TestCase("0.0 == 1", false)]
        [TestCase("1 == 1", true)]
        [TestCase("1 != 1", false)]
        [TestCase("1 < 1", false)]
        [TestCase("1 <= 1", true)]
        [TestCase("1 > 1", false)]
        [TestCase("1 >= 1", true)]
        [TestCase("1 == 2", false)]
        [TestCase("1 != 2", true)]
        [TestCase("1 < 2", true)]
        [TestCase("1 <= 2", true)]
        [TestCase("1 > 2", false)]
        [TestCase("1 >= 2", false)]
        [TestCase("2 == 1", false)]
        [TestCase("2 != 1", true)]
        [TestCase("2 < 1", false)]
        [TestCase("2 <= 1", false)]
        [TestCase("2 > 1", true)]
        [TestCase("2 >= 1", true)]
        [TestCase("1.2 == 1.2", true)]
        [TestCase("1.2 != 1.2", false)]
        [TestCase("1.2 < 1.2", false)]
        [TestCase("1.2 <= 1.2", true)]
        [TestCase("1.2 > 1.2", false)]
        [TestCase("1.2 >= 1.2", true)]
        [TestCase("1.2 == 1.3", false)]
        [TestCase("1.2 != 1.3", true)]
        [TestCase("1.2 < 1.3", true)]
        [TestCase("1.2 <= 1.3", true)]
        [TestCase("1.2 > 1.3", false)]
        [TestCase("1.2 >= 1.3", false)]
        [TestCase("1.3 == 1.2", false)]
        [TestCase("1.3 != 1.2", true)]
        [TestCase("1.3 < 1.2", false)]
        [TestCase("1.3 <= 1.2", false)]
        [TestCase("1.3 > 1.2", true)]
        [TestCase("1.3 >= 1.2", true)]
        [TestCase("1.2 == 1", false)]
        [TestCase("1.2 != 1", true)]
        [TestCase("1.2 < 1", false)]
        [TestCase("1.2 <= 1", false)]
        [TestCase("1.2 > 1", true)]
        [TestCase("1.2 >= 1", true)]
        [TestCase("true == true", true)]
        [TestCase("true == false", false)]
        [TestCase("false == false", true)]
        [TestCase("false == true", false)]
        [TestCase("true != true", false)]
        [TestCase("true != false", true)]
        [TestCase("false != false", false)]
        [TestCase("false != true", true)]
        [TestCase("\"bbb\" == \"bbb\"", true)]
        [TestCase("\"bbb\" != \"bbb\"", false)]
        [TestCase("\"bbb\" < \"bbb\"", false)]
        [TestCase("\"bbb\" <= \"bbb\"", true)]
        [TestCase("\"bbb\" > \"bbb\"", false)]
        [TestCase("\"bbb\" >= \"bbb\"", true)]
        [TestCase("\"bbb\" == \"bba\"", false)]
        [TestCase("\"bbb\" != \"bba\"", true)]
        [TestCase("\"bbb\" < \"bba\"", false)]
        [TestCase("\"bbb\" <= \"bba\"", false)]
        [TestCase("\"bbb\" > \"bba\"", true)]
        [TestCase("\"bbb\" >= \"bba\"", true)]
        [TestCase("\"bba\" == \"bbb\"", false)]
        [TestCase("\"bba\" != \"bbb\"", true)]
        [TestCase("\"bba\" < \"bbb\"", true)]
        [TestCase("\"bba\" <= \"bbb\"", true)]
        [TestCase("\"bba\" > \"bbb\"", false)]
        [TestCase("\"bba\" >= \"bbb\"", false)]
        [TestCase("\"bbb\" == \"bbbb\"", false)]
        [TestCase("\"bbb\" != \"bbbb\"", true)]
        [TestCase("\"bbb\" < \"bbbb\"", true)]
        [TestCase("\"bbb\" <= \"bbbb\"", true)]
        [TestCase("\"bbb\" > \"bbbb\"", false)]
        [TestCase("\"bbb\" >= \"bbbb\"", false)]
        [TestCase("\"bbb\" == 0", null)]
        [TestCase("\"bbb\" == -2.0", null)]
        [TestCase("1 == \"bbb\"", null)]
        [TestCase("2.0 == -2.0", false)]
        public void TestIsTrue(string input, bool? expected)
        {
            var tokenizer = Tokenizer.CreateTokenizer(input);
            var expr = ExpressionBase.Parse(new PositionalTokenizer(tokenizer));

            var scope = new InterpreterScope(AchievementScriptInterpreter.GetGlobalScope());
            scope.Context = new RATools.Parser.TriggerBuilderContext();
            ParseErrorExpression error;
            Assert.That(expr.IsTrue(scope, out error), Is.EqualTo(expected));
            Assert.That(error, Is.Null);
        }
    }
}
