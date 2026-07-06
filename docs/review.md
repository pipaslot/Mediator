# Wiki dokumentace – Diátaxis review

Tento soubor slouží jako pracovní review dokument pro `docs/wiki/`. Není součástí publikované wiki (viz `docs/wiki/` sync pravidlo v `CLAUDE.md`), takže sem lze bezpečně zapisovat poznámky, nálezy a rozpracované review bez rizika, že to smaže nightly sync na GitHub Wiki.

Review vychází z frameworku **Diátaxis** (Tutorial / How-to / Reference / Explanation)

---

## Metodika (závazná pro každého reviewera/agenta)

Pro každou sekci/kapitolu vždy vyhodnoť těchto 5 otázek:

1. **Chybí úvod/kontext?** – Je jasné proč a kdy toto téma řešit, než čtenář narazí na kód/konfiguraci?
2. **Chybí předpoklady?** – Je uvedeno, co má čtenář znát/mít nastaveno předem (např. jiná kapitola, technologie, verze)?
3. **Otevírá se téma rovnou termínem/kódem/konfigurací bez vysvětlení?** – Např. použití pojmu nebo API bez odkazu na definici.
4. **Diátaxis kategorie** – Do které kategorie (Tutorial/How-to/Reference/Explanation) sekce patří, a nemíchá se nevhodně s jinou kategorií bez oddělení?
5. **Chybí odkazy na související témata?** – Prokliky na glosář (`2.-Core-concepts-and-glossary.md`), navazující kapitoly, "Next steps"/"See also" patičku.

Navíc zaznamenávej (odděleně od Diátaxis nálezů, ale ve stejném souboru):
- **Technická přesnost** – vyloženě chybný kód, překlepy měnící význam, nekonzistentní pojmenování, prázdné sekce/nadpisy bez obsahu. Tohle není Diátaxis otázka, ale ovlivňuje důvěryhodnost dokumentace, takže se hlásí stejně přísně.

### Co review NENÍ
- Není to jazyková korektura (gramatika/styl) – pokud nenarušuje srozumitelnost.
- Není to redesign obsahu ani návrh nových kapitol – pouze chybějící kontext/předpoklady/odkazy a přesun/oddělení existujícího obsahu.
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

kde `kategorie` je jedna z: `chybí-kontext`, `chybí-předpoklady`, `bez-vysvětlení`, `diátaxis-mix`, `chybí-odkaz`, `technická-přesnost`.

Pokud ověřuješ existující nález (ne přidáváš nový), přidej k němu tag na konec řádku:
- `[POTVRZENO]` – nález platí, ověřil jsi ho nezávisle.
- `[NEPLATÍ]` – nález po ověření neobstál, napiš krátce proč.
- `[ZASTARALÉ]` – dokument se mezitím změnil a nález už neodpovídá aktuálnímu obsahu.

### Pravidla pro nezávislost review
- Nečti napřed cizí nálezy pro soubor, který teprve budeš analyzovat poprvé – analyzuj dokument samostatně podle metodiky výše a až poté (volitelně) porovnej se stávajícími nálezy pro daný soubor, pokud tam nějaké jsou. Tím zůstává review nezávislé a chyby jednoho agenta se nekopírují do dalšího.
- Pokud narazíš na potenciální chybu v kódové ukázce, over si ji vůči skutečnému chování knihovny (zdrojový kód v `Pipaslot.Mediator/`, `Pipaslot.Mediator.Http/`), ne jen podle dojmu – teprve pak ji označ jako `technická-přesnost`.
- Necituj a needituj obsah `docs/wiki/*.md` v rámci review – jen čti a zapisuj sem.
- Drž se rozsahu: pokud narazíš na věc mimo Diátaxis kritéria (např. čistě stylistická preference), nezapisuj ji jako nález, leda by šlo o srozumitelnost.

### Stav pokrytí

| Soubor | Stav | Kým/kdy |
|---|---|---|
| Home.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 1.-Why-Pipaslot.Mediator.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 2.-Core-concepts-and-glossary.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 3.-Quickstart-In-process-usage.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 4.-Quickstart-Client-Server-Blazor-WASM-usage.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 5.-Mediator-API.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 6.-Pipelines-and-Middlewares.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 6.1.-Ready-to-use-middlewares.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 7.-Authorization.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 8.-HTTP-transport-and-configuration-for-Client-Server-usage.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 9.-Advanced-usage.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 9.1.-Custom-action-and-handler-types.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 9.2.-Multi-handler-execution.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 9.3.-Custom-HTTP-responses-and-file-download.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| 10.-Cookbook-and-integrations.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |
| Release-notes-and-breaking-changes.md | HOTOVO (1. průchod) | Claude, 2026-07-05 |

Poznámka: "1. průchod" znamená, že šlo o jediného agenta v jedné konverzaci, nikoliv o nezávislé zdvojené review. Pro vyšší důvěru v nálezy je žádoucí, aby aspoň kritické (`technická-přesnost`, `diátaxis-mix`) nálezy prošly druhým, nezávislým průchodem (jiný agent/jiná konverzace) a byly označeny `[POTVRZENO]`/`[NEPLATÍ]`.

---

## Průřezová zjištění (napříč více dokumenty)

- **[diátaxis-mix]** Kapitoly 5, 6, 6.1, 7, 8 jsou vedené jako Reference, ale průběžně obsahují How-to recepty (např. "Control handler status" v 5, postup TypeNameHandling v 8) i Explanation odstavce (např. proč `Unavailable` vyhrává nad `Allow` v 7) bez vizuálního oddělení.
- **[chybí-předpoklady]** Chybí jednotný vzorec "Prerequisites" na začátku Tutorial/How-to stránek – 3, 4, 9.1, 9.3 skáčou rovnou na instalaci/kód.
- **[chybí-odkaz]** Nekonzistentní "Next steps"/"See also" patička – mají ji 1, 3, 4; chybí u 5, 6, 6.1, 7, 8, 9.2, 9.3, 10.
- **[chybí-odkaz]** Prokliky na `2.-Core-concepts-and-glossary.md` jsou nahodilé – 6 a částečně 7 odkazují na definice pojmů, ale 3, 4, 9.1, 9.2, 9.3 termíny (Handler, Action, Feature, Pipeline) používají bez odkazu.
- **[technická-přesnost]** Nalezené chyby v kódových ukázkách:
  - 3, 4: `unsigned int` není platný C# typ; handler `WheatherForecastRequestHandler` (překlep "Wheatherforecast") má metodu se signaturou `WeatherForecast.Result`/`WeatherForecast.Request`, což neodpovídá dříve deklarovaným typům `WeatherForecastRequest`/`WeatherForecastResult[]`.
  - 5: sekce "Control handler status" obsahuje osamocené slovo "Just" bez pokračování věty.
  - 8: nadpis `### Error handling` je prázdný, hned pod ním následuje `## Communication over HTTP` bez obsahu k původnímu nadpisu.
  - 9.1: `interface ICommand :  : IMediatorAction` – zdvojená dvojtečka.

## Nálezy podle dokumentu

### Home.md
- **[chybí-kontext]** Chybí klasifikace kapitol podle Diátaxis typu (Tutorial/How-to/Reference/Explanation), která by čtenáři pomohla zvolit správnou úroveň podle jeho potřeby.

### 1.-Why-Pipaslot.Mediator.md
- **[technická-přesnost]** Duplikuje první větu z Home.md ("The Mediator concept is an alternative to SOA...").
- **[chybí-kontext]** Chybí úvodní odstavec, komu a kdy je stránka určená.
- **[diátaxis-mix]** Sekce "Library structure" je fakticky Reference obsah (seznam NuGet balíčků a jejich API) vložený do Explanation kapitoly.

### 2.-Core-concepts-and-glossary.md
- **[chybí-kontext]** Chybí úvodní věta o účelu stránky (kdy do glosáře nahlížet).
- **[diátaxis-mix]** Sekce "How a call flows" (diagramy) je Explanation obsah zamíchaný do jinak čistě referenčního glosáře.
- **[chybí-odkaz]** Chybí patička s odkazem zpět na Home / dál na Quickstart.

### 3.-Quickstart-In-process-usage.md
- **[chybí-předpoklady]** Chybí uvedení předpokladů (znalost DI, C#, .NET projektová struktura).
- **[bez-vysvětlení]** Není vysvětleno, proč se odděluje "Shared" a "Executable" projekt – čtenář to musí odvodit sám.
- **[chybí-odkaz]** Termíny `IRequest<T>`, `IRequestHandler` nejsou odkázané na definice v glosáři.

### 4.-Quickstart-Client-Server-Blazor-WASM-usage.md
- **[chybí-předpoklady]** Chybí předpoklady o Blazor WASM hostování a HttpClient DI.
- **[chybí-kontext]** Chybí úvodní kontrast "kdy použít tuto kapitolu vs. in-process quickstart (3)".
- **[technická-přesnost]** Duplikuje velkou část kódu z kapitoly 3 místo odkazu zpět (riziko rozjetí obsahu při budoucích úpravách).

### 5.-Mediator-API.md
- **[diátaxis-mix]** Sekce "Control handler status" je How-to recept vložený do jinak referenční kapitoly.
- **[chybí-kontext]** Sekce "IMediatorFacade" je nepřiměřeně stručná oproti ostatním (bez příkladu použití).

### 6.-Pipelines-and-Middlewares.md
- **[diátaxis-mix]** Sekce "HandlerExistenceChecker" přerušuje logický tok mezi registrací handlerů a vysvětlením pipeline konceptu – tematicky by patřila jinam (např. Cookbook nebo samostatná sekce).
- **[chybí-odkaz]** Chybí odkaz na `Feature` v glosáři u ukázky `MiddlewareParametersFeature`.
- Poznámka: nejlépe strukturovaná kapitola z celé wiki, hodně interních odkazů na glosář – vhodný vzor pro ostatní kapitoly.

### 6.1.-Ready-to-use-middlewares.md
- **[chybí-kontext]** Chybí úvodní kontext stránky před prvním technickým odstavcem.
- **[chybí-odkaz]** `.UseNotificationReceiver()` a `.UseActionEvents()` jsou zmíněné, ale bez řádkového popisu (na rozdíl od ostatních middlewarů) – jen odkaz do Cookbooku, což narušuje úplnost referenční stránky.

### 7.-Authorization.md
- **[diátaxis-mix]** Pořadí je obrácené: praktické příklady (`[AuthenticatedPolicy]`) jsou uvedené dřív, než je vysvětlen koncept `IPolicy`/`Rule`/`RuleSet`/`RuleOutcome`, který přichází až v sekci "Custom rules" mnohem níž.
- **[diátaxis-mix]** Sekce "RuleScope" ("Policies counting with the actual model state") je vložená uprostřed handler-policy příkladů, tematicky patří spíš k vysvětlení konceptu na začátku.
- **[chybí-odkaz]** Chybí odkaz zpět na 6.1 pro `.UseAuthorization()`.
- **[chybí-předpoklady]** Chybí předpoklad obeznámenosti s ASP.NET Core ClaimsPrincipal / policy-based auth.

### 8.-HTTP-transport-and-configuration-for-Client-Server-usage.md
- **[chybí-odkaz]** Chybí patička s odkazy na konci dokumentu.
- Poznámka: sekce "TypeNameHandling and Security" je dobrým vzorem (nejdřív "proč", pak "jak") – lze použít jako šablonu pro ostatní kapitoly.

#### Revize doplněné sekce "Error handling" (řádky 42–119)
Technická přesnost obsahu byla ověřena proti zdrojovému kódu (`Pipaslot.Mediator.Http/MediatorMiddleware.cs`, `Pipaslot.Mediator.Http/Middlewares/HttpClientExecutionMiddleware.cs`, `Pipaslot.Mediator/Mediator.cs`, `Pipaslot.Mediator/Middlewares/MediatorContextExtensions.cs`): popis fallbacku HTTP status kódu, chování `HttpClientExecutionMiddleware` (nekontroluje status kód, nikdy nehází výjimku kromě cancelace/transportní chyby), rozdíl `Dispatch`/`Execute` vs. `DispatchUnhandled`/`ExecuteUnhandled`, a převod výjimky na chybovou `Notification` – to vše odpovídá skutečné implementaci. Žádný nový nález kategorie technická-přesnost.

- **[diátaxis-mix]** Sekce pod jedním nadpisem "Error handling" mísí Explanation (proč a jak mediator zachytává výjimky), Reference (přesné chování `MediatorMiddleware` a `HttpClientExecutionMiddleware` popsané bod po bodu) a How-to recepty (`ValidatorMiddleware`, `CustomLoggingMiddleware` jako kopírovatelné vzory) bez vizuálního nebo strukturálního oddělení kategorií – posiluje již zaznamenaný průřezový nález diátaxis-mix pro kapitolu 8.
- **[bez-vysvětlení]** Sekce otevírá odstavec termíny `Dispatch`/`Execute`/`DispatchUnhandled`/`ExecuteUnhandled` bez odkazu na [5.-Mediator-API.md](5.-Mediator-API.md), kde jsou tyto metody podrobně představeny – čtenář, který se do kapitoly 8 dostane přímo (např. přes vyhledávání), nemusí vědět, co tyto metody znamenají.
- **[chybí-odkaz]** Odstavec "Note that Mediator also exposes DispatchUnhandled/ExecuteUnhandled counterparts..." fakticky duplikuje vysvětlení již uvedené v `5.-Mediator-API.md` (sekce "ExecuteUnhandled and DispatchUnhandled" / "In-process exceptions"), aniž by na něj odkazoval – riziko rozjetí obsahu při budoucích úpravách (stejná kategorie rizika jako u duplicitního kódu v kapitole 4).
- **[chybí-odkaz]** Podsekce "Server: logging and notifying an administrator" popisuje middleware `ExceptionLoggingMiddleware`/`.UseExceptionLogging()`, který už je referenčně zdokumentovaný v `6.1.-Ready-to-use-middlewares.md`, ale vzájemně na sebe neodkazují.

### 9.-Advanced-usage.md
- Bez zásadních nálezů – krátký index s dobrým kontextovým úvodem ("opt-in customizations for less common scenarios").

### 9.1.-Custom-action-and-handler-types.md
- **[chybí-odkaz]** Chybí explicitní odkaz na glosář pro `IMediatorAction`/marker interfaces hned na začátku (jen sporadicky v textu).
- **[bez-vysvětlení]** Chybí vysvětlení trade-offů (kdy tento přístup nepoužívat) a přesnější odkaz na interakci s "Pipeline types" v kapitole 6.

### 9.2.-Multi-handler-execution.md
- **[diátaxis-mix]** Mísí aktuální chování s historickým kontextem ("konfigurace v pipeline byla odstraněna ve verzi 6") – historie patří spíš do Release notes, ne do referenční kapitoly.
- **[chybí-odkaz]** Chybí odkazy na související 9.1 a 6.

### 9.3.-Custom-HTTP-responses-and-file-download.md
- **[chybí-kontext]** Chybí úvodní věta o účelu/kdy toto použít, než se skočí do "Since Version 6.0.0...".
- **[diátaxis-mix]** Pořadí sekcí je obrácené: "Handler implementation" je popsaná dřív než "File download (via HTTP GET)", ačkoli druhá vysvětluje, jak se k handleru vůbec dostat (URL formatter) – logicky by mělo jít nejdřív.

### 10.-Cookbook-and-integrations.md
- Poznámka: má nejlepší úvodní větu vymezující rozsah kapitoly ze všech dokumentů ("groups middleware + UI code that consumes it") – vzorové řešení.
- **[chybí-kontext]** Jednotlivé recepty (Notifications, Events, App Insights) postrádají jednořádkové "kdy tohle použít" před přechodem ke kódu.

### Release-notes-and-breaking-changes.md
- Bez nálezů – správně strohý, chronologický changelog bez potřeby Diátaxis narativu.

---

## Po dokončení úprav
aktulizuj CLAUDE.md a přidej pravidla jež bude udržovat stejný koncept dokumentace i pro budoucí upravy
