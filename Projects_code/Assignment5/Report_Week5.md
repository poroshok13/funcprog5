# Functional Programming — Assignment 5
Student Name: Maxsutkhan Ramazanov | Group: IT-2506 | Week: 5  
Assignment Title: Composing Functions Using Pipelining  

---

## 1. Introduction and Problem Context

In e-commerce pricing engines, order totals are calculated through a series of discrete pricing rules: applying promotional discounts, adding delivery or service surcharges, and rounding to currency decimals.

In conventional imperative code, these steps are frequently executed as deeply nested function invocations:
```csharp
var price = RoundToCents(AddFee(5m, ApplyDiscount(0.10m, 1234.56m)));
```

### Problems with Nested Invocations
1. **Inside-Out Evaluation:** Readers must parse the expression from the deepest parenthesis outward, which opposes the natural chronological flow of data processing.
2. **Rigid Structure:** Adding, removing, or reordering steps requires manual rewriting of nested parentheses.
3. **Low Reusability:** Rules cannot easily be extracted, configured per customer tier, or stored in collections dynamically.

Assignment 5 refactors this calculation using foundational functional programming patterns: Currying, Partial Application, Function Composition (`Compose`), Fluent Extension Pipelining (`Pipe`), and Fold-based Dynamic Pipelines (`Pipeline` via `Aggregate`).

---

## 2. Implementation Tasks

### 2.1 Task 1: Currying and Partial Application

Currying transforms a multi-argument function into a chain of unary functions:
$$(A \times B \to C) \Longrightarrow (A \to (B \to C))$$

```csharp
public static Func<A, Func<B, C>> Curry<A, B, C>(Func<A, B, C> f)
{
    return a => b => f(a, b);
}
```

Using `Curry`, binary operations `ApplyDiscount(rate, amount)` and `AddFee(fee, amount)` are curried. Passing configuration values (`rate = 0.10m`, `fee = 5m`) produces unary functions of type `Func<decimal, decimal>` via partial application:

```csharp
var curriedDiscount = Curry<decimal, decimal, decimal>(ApplyDiscount);
var curriedFee = Curry<decimal, decimal, decimal>(AddFee);

Func<decimal, decimal> discount10 = curriedDiscount(0.10m);
Func<decimal, decimal> fee5 = curriedFee(5m);
```

---

### 2.2 Task 2: Function Composition (`Compose`)

Forward composition chains two functions such that the output of $f$ becomes the input to $g$:
$$(f \ggg g)(x) = g(f(x))$$

```csharp
public static Func<A, C> Compose<A, B, C>(Func<A, B> f, Func<B, C> g)
{
    return x => g(f(x));
}
```

A complete reusable pricing function `checkout` is built by composing `discount10`, `fee5`, and `RoundToCents`:

```csharp
Func<decimal, decimal> checkout = Compose(Compose(discount10, fee5), round);
```

For input `1234.56m`, evaluating `checkout(1234.56m)` produces `1116.10`.

---

### 2.3 Task 3: Fluent Pipelining Extension (`Pipe`)

To achieve left-to-right syntax directly on values, an extension method is defined:

```csharp
public static class PipeExtensions
{
    public static R Pipe<T, R>(this T value, Func<T, R> f)
    {
        return f(value);
    }
}
```

This transforms the nested call into a readable linear pipeline:

```csharp
decimal result = 1234.56m
    .Pipe(discount10)
    .Pipe(fee5)
    .Pipe(round);
```

Execution trace for `1234.56m`:
1. `1234.56m.Pipe(discount10)` $\to$ `1111.104`
2. `1111.104m.Pipe(fee5)` $\to$ `1116.104`
3. `1116.104m.Pipe(round)` $\to$ `1116.10`

---

### 2.4 Task 4: Dynamic Pipeline Construction with `Aggregate`

When pipeline stages are determined at runtime (e.g., from user roles or promotional campaigns), functions can be supplied in a collection and folded using `Enumerable.Aggregate`:

```csharp
public static Func<decimal, decimal> Pipeline(params Func<decimal, decimal>[] steps)
{
    return steps.Aggregate(
        (Func<decimal, decimal>)(x => x),
        (acc, step) => x => step(acc(x))
    );
}
```

#### Comparison of Execution Orders on Amount `1234.56` (Printed without `F2`):

| Pipeline Configuration | Sequence of Functions | Raw Decimal Output | Explanation |
|:---|:---|:---|:---|
| **1. Standard** | `discount10` $\to$ `fee5` $\to$ `round` | `1116.10` | 10% discount on order only; fee added in full afterwards; cents rounded. |
| **2. Swapped** | `fee5` $\to$ `discount10` $\to$ `round` | `1115.60` | Fee is added first, so discount also reduces the fee by 10% ($5 \times 0.10 = 0.50$ cheaper). |
| **3. Round First** | `round` $\to$ `discount10` $\to$ `fee5` | `1116.1040` | Rounding executed when input already has 2 decimals; subsequent discount produces 4 decimal places which remain unrounded. |
| **4. Empty Pipeline** | `Pipeline()` | `1234.56` | Returns the identity function; input passes through unaltered. |

---

## 3. Execution Output

```text
Assignment 5 - Composing Functions Using Pipelining
Maxsutkhan Ramazanov | IT-2506

--- Task 1: Currying & Partial Application ---
Initial Amount: 1234,56
After 10% discount: 1111,1040
After 5 flat fee: 1116,1040
After rounding: 1116,10

--- Task 2: Function Composition ---
Order amount: 1234,56
Checkout result: 1116,10

--- Task 3: Pipelining with Extension Method ---
Order amount: 1234,56
Piped result: 1116,10

--- Task 4: Dynamic Pipeline with Aggregate ---
Input amount: 1234,56
1. Standard (discount -> fee -> round): 1116,10
2. Swapped (fee -> discount -> round):   1115,60
3. Round First (round -> discount -> fee): 1116,1040
4. Empty Pipeline ():                     1234,56
```

---

## 4. Answers to Assignment Questions

### Question 1
> **Why must every step be a pure function for composition to be safe? Give an example of a step in a payment system that would break it.**

* **Why purity is required:**
  Function composition assumes referential transparency and associativity:
  $$(f \circ g) \circ h = f \circ (g \circ h)$$
  If a step contains side effects (e.g., mutating global variables, performing database mutations, or depending on non-deterministic state like the system clock), reordering, lazy execution, or retrying pipelines becomes unsafe. An impure function can cause unintended secondary side effects when invoked as part of a composed expression.
* **Payment system example:**
  Consider a step `ChargeCreditCard(decimal amount)`. If embedded directly within a pricing composition pipeline, evaluating the pipeline for preview, quoting, or debugging would immediately capture funds from the customer's bank account. Any replay or rearrangement of the pipeline would trigger duplicate credit card transactions.

---

### Question 2
> **What is the difference between currying and partial application? Why are rate and fee the first parameters and amount the last one?**

* **Difference:**
  * **Currying:** A structural transformation of a function with $n$ arguments into a chain of $n$ functions that each take exactly one argument ($f(a, b) \to f_a(b)$).
  * **Partial Application:** Supplying a subset of arguments to a function upfront, yielding a new function that expects only the remaining parameters.
* **Parameter ordering rationale (`rate`/`fee` first, `amount` last):**
  In functional design, static configuration parameters (constants, policies, rates) are placed first, while the dynamic data operand (`amount`) is placed last. This allows callers to partially apply configuration settings once and obtain a clean unary function (`decimal -> decimal`) that fits seamlessly into pipeline chains without glue code.

---

### Question 3
> **Why do the pipelines give different results? What does the rounding-first pipeline show about why rounding is the last step?**

* **Why results differ:**
  Linear operations and affine transformations do not commute. Specifically:
  $$\text{discount}(\text{fee}(x)) = (x + 5) \cdot 0.9 = 0.9x + 4.5$$
  $$\text{fee}(\text{discount}(x)) = (x \cdot 0.9) + 5 = 0.9x + 5.0$$
  When fee is applied first, the subsequent discount reduces both the order subtotal and the fee itself by 10%, resulting in an amount that is $0.50$ lower ($1115.60$ vs $1116.10$).
* **Significance of the Round-First pipeline:**
  When `RoundToCents` is moved to the front, it rounds the already clean currency figure `1234.56`. Then `discount10` multiplies by `0.90`, producing `1111.1040` (fractions of a cent). Because no further rounding occurs, the final price retains fractional cents (`1116.1040`). This demonstrates that rounding must always be the terminal step in financial pipelines to sanitize intermediate fractional values before persistence or billing.

---

### Question 4
> **What does each of Compose, Pipe and Pipeline give you that the nested call does not? What does Pipeline() return, and why does Aggregate need the identity function as its seed for that?**

* **Advantages over nested calls:**
  * `Compose`: Enables point-free style, allowing developers to construct reusable composite functions from atomic building blocks without referencing intermediate values.
  * `Pipe`: Provides natural, linear, left-to-right reading order, eliminating parentheses nesting and matching chronological data flow.
  * `Pipeline`: Enables runtime dynamism; the list of applied transformations can be loaded from configuration, database rules, or conditional filters as an array or collection.
* **Return value and seed role of `Pipeline()`:**
  * When invoked with no parameters, `Pipeline()` returns the identity function: `x => x`.
  * `Aggregate` requires the identity function as its seed because identity is the neutral (identity) element of function composition:
    $$f \circ \text{id} = \text{id} \circ f = f$$
    When the input sequence of steps is empty, folding over the seed yields `x => x`, which safely returns the unmodified input amount without throwing exceptions or requiring conditional branches.
