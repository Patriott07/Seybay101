using System.Collections.Generic;

public class DayRules
{
    public static List<RuleViolation> GetActiveRules(int day)
    {
        var rules = new List<RuleViolation>();

        // Base rules — all days have these
        rules.Add(new ExpiredPassportViolation());
        rules.Add(new InfoMismatchViolation());
        rules.Add(new StandardViolation());

        // Day 2+ — inherits Day 1 + adds Bribery
        if (day >= 2)
        {
            rules.Add(new NewRule_Day2());
        }

        // Day 3+ — adds new rule
        if (day >= 3)
        {
            rules.Add(new NewRule_Day3());
        }

        // Day 5+ — adds new rule
        if (day >= 5)
        {
            rules.Add(new NewRule_Day5());
        }

        // Day 7+ — adds new rule
        if (day >= 7)
        {
            rules.Add(new NewRule_Day7());
        }

        // Day 9+ — adds new rule
        if (day >= 9)
        {
            rules.Add(new NewRule_Day9());
        }

        return rules;
    }
}
