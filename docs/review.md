# Wiki dokumentace – Diátaxis review

Tento soubor slouží jako pracovní review dokument pro `docs/wiki/`. Není součástí publikované wiki (viz `docs/wiki/` sync pravidlo v `CLAUDE.md`), takže sem lze bezpečně zapisovat poznámky, nálezy a rozpracované review bez rizika, že to smaže nightly sync na GitHub Wiki.

Review vychází z frameworku **Diátaxis** (Tutorial / How-to / Reference / Explanation)

---

## Metodika (závazná pro každého reviewera/agenta)

Pro každou sekci/kapitolu vždy vyhodnoť těchto 3 otázky:

1. **Otevírá se téma rovnou termínem/kódem/konfigurací bez vysvětlení?** – Např. použití pojmu nebo API bez odkazu na definici.
2. **Diátaxis kategorie** – Do které kategorie (Tutorial/How-to/Reference/Explanation) sekce patří, a nemíchá se nevhodně s jinou kategorií bez oddělení?
3. **Chybí odkazy na související témata?** – Prokliky na glosář (`2.-Core-concepts-and-glossary.md`), navazující kapitoly, "Next steps"/"See also" patičku.

Navíc zaznamenávej (odděleně od Diátaxis nálezů, ale ve stejném souboru):
- **Technická přesnost** – vyloženě chybný kód, překlepy měnící význam, nekonzistentní pojmenování, prázdné sekce/nadpisy bez obsahu. Tohle není Diátaxis otázka, ale ovlivňuje důvěryhodnost dokumentace, takže se hlásí stejně přísně.

### Co review NENÍ
- Není to jazyková korektura (gramatika/styl) – pokud nenarušuje srozumitelnost.
- Není to redesign obsahu ani návrh nových kapitol – pouze chybějící odkazy a přesun/oddělení existujícího obsahu.
- Žádné úpravy souborů v `docs/wiki/` v této fázi. Návrhy se zapisují pouze sem, do `docs/review.md`.

---

## Instrukce pro nezávislé review agenty

Tato sekce je určená agentovi, který nemá kontext z předchozí konverzace a dostane za úkol pokrýt review dalších kapitol (nebo nezávisle verifikovat existující nálezy).

### Rozdělení práce
Wiki má 16 souborů v `docs/wiki/`. Aby se práce více agentů nepřekrývala:
1. Před spuštěním review zkontroluj sekci **"Stav pokrytí"** níže – najdi soubory se stavem `TODO` nebo `NEOVĚŘENO`, a vezmi si jen ty.
2. Po dokončení review jednoho souboru okamžitě aktualizuj jeho stav ve "Stav pokrytí" (viz níže), ať další agent neduplikuje práci.
3. Pokud je ti přiděleno more souborů najednou, zpracuj je sekvenčně a commituj/ukládej průběžně (přidávej do tohoto souboru, needituj cizí nálezy).

### Formát zápisu nálezu
Každý nález zapisuj jako položku pod příslušný soubor v sekci "Nálezy podle dokumentu", v tomto tvaru:

```
- **[kategorie]** stručný popis nálezu (1–2 věty). Umístění: sekce/nadpis nebo řádek.
```

kde `kategorie` je jedna z: `bez-vysvětlení`, `diátaxis-mix`, `chybí-odkaz`, `technická-přesnost`.

Pokud ověřuješ existující nález (ne přidáváš nový), přidej k němu tag na konec řádku:
- `[POTVRZENO]` – nález platí, ověřil jsi ho nezávisle.
- `[NEPLATÍ]` – nález po ověření neobstál, napiš krátce proč.
- `[ZASTARALÉ]` – dokument se mezitím změnil a nález už neodpovídá aktuálnímu obsahu.

### Pravidla pro nezávislost review
- Nečti napřed cizí nálezy pro soubor, který teprve budeš analyzovat poprvé – analyzuj dokument samostatně podle metodiky výše a až poté (volitelně) porovnej se stávajícími nálezy pro daný soubor, pokud tam nějaké jsou. Tím zůstává review nezávislé a chyby jednoho agenta se nekopírují do dalšího.
- Pokud narazíš na potenciální chybu v kódové ukázce, over si ji vůči skutečnému chování knihovny (zdrojový kód v `Pipaslot.Mediator/`, `Pipaslot.Mediator.Http/`), ne jen podle dojmu – teprve pak ji označ jako `technická-přesnost`.
- Necituj a needituj obsah `docs/wiki/*.md` v rámci review – jen čti a zapisuj sem.
- Drž se rozsahu: pokud narazíš na věc mimo Diátaxis kritéria (např. čistě stylistická preference), nezapisuj ji jako nález, leda by šlo o srozumitelnost.


## Průřezová zjištění (napříč více dokumenty)

- **[diátaxis-mix]** Kapitoly 5, 6, 6.1, 7, 8 jsou vedené jako Reference, ale průběžně obsahují How-to recepty (např. "Control handler status" v 5, postup TypeNameHandling v 8) i Explanation odstavce (např. proč `Unavailable` vyhrává nad `Allow` v 7) bez vizuálního oddělení.

## Nálezy podle dokumentu

### 5.-Mediator-API.md
- **[diátaxis-mix]** Sekce "Control handler status" je How-to recept vložený do jinak referenční kapitoly.

### 6.-Pipelines-and-Middlewares.md
- **[diátaxis-mix]** Sekce "HandlerExistenceChecker" přerušuje logický tok mezi registrací handlerů a vysvětlením pipeline konceptu – tematicky by patřila jinam (např. Cookbook nebo samostatná sekce).
- Poznámka: nejlépe strukturovaná kapitola z celé wiki, hodně interních odkazů na glosář – vhodný vzor pro ostatní kapitoly.

### 7.-Authorization.md
- **[diátaxis-mix]** Pořadí je obrácené: praktické příklady (`[AuthenticatedPolicy]`) jsou uvedené dřív, než je vysvětlen koncept `IPolicy`/`Rule`/`RuleSet`/`RuleOutcome`, který přichází až v sekci "Custom rules" mnohem níž.
- **[diátaxis-mix]** Sekce "RuleScope" ("Policies counting with the actual model state") je vložená uprostřed handler-policy příkladů, tematicky patří spíš k vysvětlení konceptu na začátku.

### 8.-HTTP-transport-and-configuration-for-Client-Server-usage.md
- Poznámka: sekce "TypeNameHandling and Security" je dobrým vzorem (nejdřív "proč", pak "jak") – lze použít jako šablonu pro ostatní kapitoly.

#### Revize doplněné sekce "Error handling" (řádky 42–119)
Technická přesnost obsahu byla ověřena proti zdrojovému kódu (`Pipaslot.Mediator.Http/MediatorMiddleware.cs`, `Pipaslot.Mediator.Http/Middlewares/HttpClientExecutionMiddleware.cs`, `Pipaslot.Mediator/Mediator.cs`, `Pipaslot.Mediator/Middlewares/MediatorContextExtensions.cs`): popis fallbacku HTTP status kódu, chování `HttpClientExecutionMiddleware` (nekontroluje status kód, nikdy nehází výjimku kromě cancelace/transportní chyby), rozdíl `Dispatch`/`Execute` vs. `DispatchUnhandled`/`ExecuteUnhandled`, a převod výjimky na chybovou `Notification` – to vše odpovídá skutečné implementaci. Žádný nový nález kategorie technická-přesnost.

- **[diátaxis-mix]** Sekce pod jedním nadpisem "Error handling" mísí Explanation (proč a jak mediator zachytává výjimky), Reference (přesné chování `MediatorMiddleware` a `HttpClientExecutionMiddleware` popsané bod po bodu) a How-to recepty (`ValidatorMiddleware`, `CustomLoggingMiddleware` jako kopírovatelné vzory) bez vizuálního nebo strukturálního oddělení kategorií – posiluje již zaznamenaný průřezový nález diátaxis-mix pro kapitolu 8.
