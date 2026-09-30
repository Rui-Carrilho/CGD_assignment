### 1 - **Explique, por palavras suas, como interpreta as regras de negócio.**

As regras permitem automatizar a avaliação de pedidos de empréstimo, verificando se os dados fornecidos são válidos e se o pedido reúne as condições de elegibilidade para o respetivo empréstimo.


The rules automate the preliminary assessment of personal-loan requests, checking whether the supplied data is valid and whether the request meets the defined eligibility and affordability criteria.
Requests with invalid input receive PEDIDO INVÁLIDO. 
Valid requests that meet a refusal condition receive RECUSADO. 
Requests that require an analyst’s judgment receive ANÁLISE MANUAL. 
Requests without any invalidity, refusal, or manual-review conditions receive APROVADO.
When several conditions apply, the most restrictive decision prevails. The application must explain the result through the applicable reasons and calculated indicators.
These rules implement the criteria supplied in the exercise; they do not guarantee that an approved applicant will repay the loan.

### 2 - Descreva a ordem pela qual pretende aplicar as regras
First, I would validate the input according to Rule 1 and collect all validation errors. If validation fails, I would return PEDIDO INVÁLIDO without performing the financial calculations.
For valid requests, I would calculate the estimated installment, effort rate, and estimated age at the end of the contract. I would then evaluate Rules 2–7, collecting each applicable reason.
Finally, I would apply Rule 8 to select the most restrictive result, in the following order:
PEDIDO INVÁLIDO > RECUSADO > ANÁLISE MANUAL > APROVADO
If no rule requires refusal or manual analysis, the result is APROVADO.

### 3 - Identifique possíveis situações não previstas ou ambíguas.
Missing fields or unsupported values - The document does not define the result for missing employment status or an unanswered credit-incidents field.
Negative existing instalments -	This is not explicitly prohibited, but would artificially reduce the effort rate.
Decimal age or term - It is not stated explicitly whether age and term must be whole numbers.
NIF validity - Does “nine digits” suffice, or is additional validation intended?
Age at contract end - Whole-year age cannot provide an exact maturity age without a birth date and contract start date.
Rounding - Rounding before comparison can change a decision near 35% or 50%.
Rule 7 versus Rule 8 - “Regardless of other rules” could suggest manual review overrides refusal, contradicting the stated priority.
Reasons returned - Should the result include every triggered reason or only reasons supporting the winning decision?
Manual-analysis workflow - Who reviews the request, which outcomes are possible, and how are changes recorded?
Reporting terminology - Last month” and “terminated” are not defined precisely.

### 4- Indique que questões colocaria ao analista de negócio para clarificar os requisitos
##### **If a request exceeds €50,000 and also meets a refusal condition, which result wins?**
Refusal wins, following Rule 8.
##### **Should missing fields, negative existing instalments, and unknown employment values make the request invalid?**	
Yes; age and term must also be whole numbers.
###### **Is NIF validation limited to exactly nine digits?**
Yes; no additional checksum validation.
##### **How should age at contract end be calculated with the available input?**
Compare Idade × 12 + PrazoMeses against 75 × 12, acknowledging the approximation.
##### **Should rounding happen before the effort-rate comparison?**	
No; compare using calculation precision and round only for display.
##### **Should all triggered reasons be returned?**	
Yes, identifying each reason’s associated decision level.
##### **Does approval mean approval of this preliminary assessment or final authorization to grant the loan?**	
Approval within this component’s preliminary assessment.
##### **Who may resolve manual review, and what transitions are permitted?**	
An authorized analyst may approve or refuse, recording a justification.
##### **Does “last month” mean the previous calendar month or a rolling period?**	
Previous calendar month, with an agreed reporting timezone.
##### **For refusal statistics, should all refusal reasons count when several apply?**	
Count each distinct refusal reason once per refused request; allow ties.
##### **Does manual-review-to-approved reporting include requests later moved to another status?**
Confirm whether it means currently approved or any historical transition to approval.

### 5 - Conseguiria apresentar uma proposta de solução no formato de user story?

**Title:** Automatically assess a personal-loan request

#### **As a** credit analyst,  
#### **I want** the application to assess a personal-loan request using the defined business rules,  
**so that** I can obtain a consistent preliminary decision, understand its reasons, and identify requests requiring manual review.