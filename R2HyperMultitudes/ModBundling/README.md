# Hyper Multitudes

It's Multitudes, but it scales up every stage!

Based on the existing [Multitudes](https://thunderstore.io/package/wildbook/Multitudes/) mod. Just like Multitudes, this mod is Server-side only, not everyone needs to have it installed.

### DISCLAIMER: Just like regular multitudes, setting a very high multitudes multiplier will result in insanely high loading times and potentially some big amounts of lag.

## Technical Stuff

Multitudes artificially increases the "amount of players in a game" using an arbitrary multiplier you can set with a console command.
This mod does pretty much the same thing (theoretically making it "multitudes compatible"), but you can set an arbitrary math equation which will be evaluated every stage, yielding the new "Multitudes multiplier" for the stage.

## Setup

The mod uses console commands for configuration.
While you can edit the config yourself or using r2modman, I **highly discourage it**, as the mod won't load with an incorrect math expression.

| Syntax                                 | Description                                                                                                                                       | Example                             |
|----------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------|
| `mod_hm_enable`                        | Enables HyperMultitudes                                                                                                                           | `mod_hm_enable`                     |
| `mod_hm_disable`                       | Disables HyperMultitudes                                                                                                                          | `mod_hm_disable`                    |
| `mod_hm_get_expression`                | Get the current HyperMultitudes Expression                                                                                                        | `mod_hm_get_expression`             |
| `mod_hm_set_expression <expression>`   | Set a new HyperMultitudes Expression. This command tests the given expression for potential errors. **Make sure the expression is put in quotes** | `mod_hm_set_expression "2 * stage"` |
| `mod_hm_test_expression <stage index>` | Tests the current HyperMultitudes Expression with the given stage index                                                                           | `mod_hm_test_expression 3`          |

## About Expressions

Honestly I don't remember why I thought it was a great idea to add math expressions for this mod's config, but now that it is done, just deal with it.

Here is a list of all operations implemented in the math parser and the corresponding syntax:
- Any whitespace character is automatically skipped
- Numbers with decimals (any chain of numbers, use `.` for decimals)
- Additions (`+`)
- Subtractions (`-`)
- Multiplication (`*`)
- Division (`/`)
- Exponentiation (`^`)
- Parentheses and operation priorities (`(` and `)`)
- Variables (only `stage` and `x` are valid in this context, both representing the current stage number)
- I might implement some more math functions and operations later, let me know if I should (you're insane if you do)

The math parser was implemented by following [this Medium article](https://medium.com/@toptensoftware/writing-a-simple-math-expression-engine-in-c-d414de18d4ce). You can call me lazy if you want, but to be fair I also learned quite a bit.

## Potential Issues/Known Issues

- Using regular multitudes (or any other multitudes mod) along with this mod *might* potentially cause a softlock on the final boss trigger due to how I implemented higher precision multipliers.

## Version History

### v1.0.0
Initial Release
