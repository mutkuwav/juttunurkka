# Juttunurkka

Juttunurkka on Android-mobiilisovellus luokkahuonekäyttöön. Sovelluksen avulla opettaja voi kerätä oppilailta anonyymejä tunnetilaäänestyksiä ja järjestää aktiviteettiäänestyksiä reaaliajassa.

## Toiminta lyhyesti

**Opettaja** kirjautuu sisään, luo kyselyn ja avaa istunnon. Sovellus generoi huonekoodin, jonka oppilaat syöttävät omille laitteillaan liittyäkseen kyselyyn.

**Oppilaat** valitsevat emojin, joka kuvaa heidän sen hetkistä tunnetilaansa. Emojivaihtoehdot ovat:
- Iloinen, Hämmästynyt, Neutraali, Vihainen, Väsynyt, Miettivä, Itkunauru ja Oma emoji (piirrettävä)

Emojin valinnan jälkeen opettaja näkee tulokset ja voi käynnistää aktiviteettikierroksen, jossa oppilaat äänestävät ehdotetuista toimenpiteistä.

## Rakenne

```
bachelors-project-2021/
├── Prototype/Prototype/        # .NET MAUI Android -sovellus (pääprojekti)
│   ├── LuoKysely/             # Kyselyn luontinäkymät (opettaja)
│   ├── VastaaKyselyyn/        # Vastausnäkymät (oppilas)
│   ├── KyselynTulokset/       # Tulosnäkymät
│   ├── TallennetutKyselyt/    # Tallennettujen kyselyjen hallinta
│   ├── AktiviteettiÄänestys/  # Aktiviteettikierros
│   └── Services/              # REST API -kommunikaatio
├── Prototype/Prototype.Droid/ # Android-kohdealustaprojekti
├── PrototypeUnitTest/         # Yksikkötestit
└── PrototypeUITestAndroid/    # UI-testit
```


## Backend

Sovellus kommunikoi REST API -backendiin. Osoite konfiguroidaan tiedostossa [ApiConfig.cs](Prototype/Prototype/ApiConfig.cs):

```csharp
public static string BaseUrl = "http://86.50.20.47:8080/";
```



## Kehittäminen

### Vaatimukset

### Käynnistys


### Opettajan kirjautuminen (prototyyppi)

Opettajan sisäänkirjautumisessa käytetään kovakoodattuja tunnuksia:

- Käyttäjätunnus: `opettaja`
- Salasana: `opehuone`

## Tekijät

- 2026 Lauri Romppainen, Elias Tupasela, Matias Meriläinen, Aurora Kansanoja, Mimosa Joenväärä

## Lisenssi

GNU General Public License v3.0 — katso [LICENSE](https://www.gnu.org/licenses/gpl-3.0.html)
