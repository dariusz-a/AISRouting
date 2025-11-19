# Select a Winner (uses your trade-off matrix)

## Task

Create a folder named docs/tech_design/architecture (if it doesn't already exist).

You MUST Write a detailed, comprehensive, Selected Winner  document in `docs/tech_design/architecture/selected_winner.md`

You MUST update any existing files with the same name.

Help me pick a provisional winner. 
1) Score each alternative against QAs using my evidence only. 
2) Call out where my evidence is weak or missing. 
3) Recommend a winner 
4) List the top 2 validation steps to de-risk the winner.


### Rules
- Keep it concise and cite the lines you used from my notes.
- Scoring across QAs:
    - Scale: High=3, Medium=2, Low=1. 
    - “Delivery Risk” uses “Low” as best; score accordingly.

## Input Sources

### Alernative architecures — Trade-off Matrix

- Location: `docs/tech_design/architecture/alt_arch_matrix.md`
- Trade-off Matrix

    - A markdown table with the bleo structure..
| Attribute | Candidate | Candidate B | Candidate C |
|---|---|---|---|

- Summary

- A paragraph summarizing dominant trade-offs:
    - Candidate A
    - Candidate B
    - Candidate C

## Output Requirements:


### **CRITICAL: Focus on Completeness, Clarity and Alignment**

Readiness Check:
- Winner justified with QA weights, trade-off evidence, and proposed validation steps.

The document MUST follow this structure:

```markdown
# Selected Winner — Architecture Trade‑off Decision 

```

The document MUST be structured with the following sections:

## QA scoring (evidence‑based)

- A markdown table with the bleo structure..
| Attribute | Candidate A Score | Candidate B Score | Candidate C Score |
|---|---|---|---|

- Totals: 

## Where evidence is weak or missing


## Recommendation (provisional)


## Top 2 validation steps to de‑risk [selected candidate]


## Appendix — Source summary excerpts
- All citations refer to alt_arch_matrix.md line numbers.

For example:

Candidate A

“[quote exact phrase A]” [L19]
“[quote exact phrase B]” [L10–L12]

Candidate B

“[quote exact phrase]” [L20]
“[quote exact phrase or paraphrase]” [L9, L11–L12]

Candidate C

“[quote exact phrase]” [L21]
“[quote exact phrase or paraphrase]” [L14]

Quote short, exact phrases; use en dashes for ranges: for example [L11–L14].
For paraphrases, still cite lines used (single or range).
If quoting from multiple files, use filename + line(s): [alt_arch_matrix.md:L9–L12].