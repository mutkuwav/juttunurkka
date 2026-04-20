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
Projektin client puolta kehitetään .NET MAUI -teknologialla käyttäen C#:ia ja XAML:ia.
Projektin backend on toteutettu javalla. 


### Vaatimukset

- Android SDK ja emulaattori TAI fyysinen Android-laite
- Internet-yhteys
- VM-serveri, jossa backend pyörii.


### Opettajan kirjautuminen (prototyyppi)

Opettajan sisäänkirjautumisessa käytetään kovakoodattuja tunnuksia:

- Käyttäjätunnus: `opettaja`
- Salasana: `opehuone`


## Tekijät

- 2026 Lauri Romppainen, Elias Tupasela, Matias Meriläinen, Aurora Kansanoja, Mimosa Joenväärä

## Lisenssi

GNU General Public License v3.0 — katso [LICENSE](https://www.gnu.org/licenses/gpl-3.0.html)


BACKEND README:

Backend korvaa vanhan LAN-vain (UDP/TCP) -toteutuksen HTTP(S)-rajapinnalla, jotta opettaja ja oppilaat voivat käyttää sovellusta internetin yli (esim. CSC cPouta VM:llä).

Päätoiminnot:

Opettaja luo uuden “huoneen/istunnon” (room) ja asettaa kyselyn (intro + emoji-vaihtoehdot)
Oppilaat liittyvät huoneeseen avainkoodilla ja äänestävät emojin (1 ääni / laite)
Backend palauttaa emojituloslaskennan
Opettaja käynnistää aktiviteettiäänestyksen, oppilaat äänestävät (1 ääni / laite)
Oppilaat voivat ladata itse piirretyn emojin PNG-muodossa, ja opettaja voi nähdä listan + avata kuvat
Tekniset speksit:

Ohjelma on ns. "pure javaa", ilman ulkopuolisia kirjastoja
com.sun.net.httpserver.HttpServer (Javan oma kevyt HTTP-palvelin)
JSON käsitellään yksinkertaisella parsinnalla (JsonUtil)
Testaus: curl + bash test.sh
Palvelin käynnistyy osoitteeseen: http://localhost:8080

Vaatimukset:

Java JDK (testattu: Java 21)
curl
Windowsissa: Git Bash (test.sh:n ajamiseen)
Ohjelman testaaminen: Avaa Command Prompt backend-kansiossa (missä Main.java on):

javac --add-modules jdk.httpserver *.java
java --add-modules jdk.httpserver -cp . Main
Huom. Tarkemmat ohjeet test.sh luokan sisällä.

