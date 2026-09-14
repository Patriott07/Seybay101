using System.Collections.Generic;

// ============================================================
// INHERITANCE CHAIN — Rule Hierarchy per Day
// ============================================================
// Day1RuleSet → 3 base rules (all days start here)
// Day2RuleSet → inherits Day1 + adds Bribery (4 rules)
// Day3RuleSet → inherits Day2 + adds Suspicious Origin (5 rules)
// Day4RuleSet → inherits Day3 (no new rules, same 5)
// Day5RuleSet → inherits Day4 + adds Counterfeit (6 rules)
// Day6RuleSet → inherits Day5 (no new rules, same 6)
// Day7RuleSet → inherits Day6 + adds Blacklist (7 rules)
// Day8RuleSet → inherits Day7 (no new rules, same 7)
// Day9RuleSet → inherits Day8 + adds Mutation Marker (8 rules)
// Day10RuleSet → inherits Day9 (no new rules, same 8)
// ============================================================

public abstract class DayRuleSet
{
    // Override this in each derived class to add new rules
    public virtual List<RuleViolation> GetRules()
    {
        return new List<RuleViolation>();
    }
}

// Day 1 — Base rules (3 rules): Expired, InfoMismatch, Standard
public class Day1RuleSet : DayRuleSet
{
    public override List<RuleViolation> GetRules()
    {
        return new List<RuleViolation>
        {
            new ExpiredPassportViolation(),     // Rule 1: Passport expired check
            new InfoMismatchViolation(),          // Rule 2: Ticket & passport must match
            new StandardViolation()               // Rule 3: Photo & validity standard
        };
    }
}

// Day 2 — Inherits Day1 + Bribery (4 rules total)
public class Day2RuleSet : Day1RuleSet
{
    public override List<RuleViolation> GetRules()
    {
        var rules = base.GetRules();           // Get Day1's 3 rules
        rules.Add(new NewRule_Day2());          // Add Bribery
        return rules;
    }
}

// Day 3 — Inherits Day2 + Suspicious Origin (5 rules total)
public class Day3RuleSet : Day2RuleSet
{
    public override List<RuleViolation> GetRules()
    {
        var rules = base.GetRules();           // Get Day2's 4 rules
        rules.Add(new NewRule_Day3());          // Add Suspicious Origin
        return rules;
    }
}

// Day 4 — Inherits Day3 (same 5 rules, no new)
public class Day4RuleSet : Day3RuleSet { }

// Day 5 — Inherits Day4 + Counterfeit Document (6 rules total)
public class Day5RuleSet : Day4RuleSet
{
    public override List<RuleViolation> GetRules()
    {
        var rules = base.GetRules();           // Get Day4's 5 rules
        rules.Add(new NewRule_Day5());          // Add Counterfeit
        return rules;
    }
}

// Day 6 — Inherits Day5 (same 6 rules, no new)
public class Day6RuleSet : Day5RuleSet { }

// Day 7 — Inherits Day6 + Blacklist Entry (7 rules total)
public class Day7RuleSet : Day6RuleSet
{
    public override List<RuleViolation> GetRules()
    {
        var rules = base.GetRules();           // Get Day6's 6 rules
        rules.Add(new NewRule_Day7());          // Add Blacklist
        return rules;
    }
}

// Day 8 — Inherits Day7 (same 7 rules, no new)
public class Day8RuleSet : Day7RuleSet { }

// Day 9 — Inherits Day8 + Mutation Marker (8 rules total)
public class Day9RuleSet : Day8RuleSet
{
    public override List<RuleViolation> GetRules()
    {
        var rules = base.GetRules();           // Get Day8's 7 rules
        rules.Add(new NewRule_Day9());          // Add Mutation Marker
        return rules;
    }
}

// Day 10 — Inherits Day9 (all 8 rules, final day)
public class Day10RuleSet : Day9RuleSet { }

// ============================================================
// RULE FACTORY — Maps day number to its corresponding DayRuleSet
// ============================================================
public static class DayRules
{
    public static List<RuleViolation> GetActiveRules(int day)
    {
        DayRuleSet ruleSet = day switch
        {
            1  => new Day1RuleSet(),
            2  => new Day2RuleSet(),
            3  => new Day3RuleSet(),
            4  => new Day4RuleSet(),
            5  => new Day5RuleSet(),
            6  => new Day6RuleSet(),
            7  => new Day7RuleSet(),
            8  => new Day8RuleSet(),
            9  => new Day9RuleSet(),
            10 => new Day10RuleSet(),
            _  => new Day1RuleSet()              // Fallback to Day1 for unknown days
        };

        return ruleSet.GetRules();
    }
}
