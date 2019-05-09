# monopol
Detta är mitt slutprodject till programmering 2
Det är en monopol variant med de flesta orginal reglerna

## Första planering och nu

Följde min plan ty helt.

Det enda jag märkte när jag började programmera var att jag hade tänkt helt fel på hur min sörver skulle fungera,
jag började med att göra så att en dator var endast sörver och alla andra datorer var klienter. Detta lede till att jag var tvunngen att behandla klienten och servern olika tack vare att de gjorde olika saker och server var tvungen att kunna göra samma sak som klienten och samtidigt göra det som sörven ska gör.
detta gjorde det väldigt svårt för mig att tänka hur programmet skulle fungerade och helt ärligt gav mig rätt mycket huvudverk. Jag valde därför att göra en dator till både server och klient samtidig fast driva dem på varsin tråd.

Anledningen till att jag inte gjorde en sörver vi sidan av var för att jag vill att det ska vara smidigt att köra lokalt om man vill köra med kompisar. Tyvär ser man detta allt sälan för idag måset man koppla upp till en server i de flesta spel för att kunna spela online men när jag var yngre kunde man alltid starta en lokal sörver och det var mitt mål att göra nu med.


min Player, Game och Plate class ser nästan identiska ut i genfört med hur jag planerade det. Det är endast + - några funktioner och variabler.


## Vad jag hade viljat lägga till(kanske kommer lägga till unde sommarlåvet)
+ möligheten att sälja plattor till andra spelare.
+ möjligheten att köra över nätverk.
+ Vissa delar i programmet är väldigt dåligt optimerade vilket gör programmet segt då och då.
+ skriva ut namn, pris, färg för varje platta på skärmen i genom att rita ut varje platta för sig.
+ visa mer exakt vilken spelare som har vilken pjäs
+ Göra det lättare för spelare att ändra i PlateInfo/ CardInfo samt skapa möjligheten att debuga om man skriver något fel i file som kan göra så att spelet inte kan starta
+ Lägga till möjligheten att ändra inställningra
+ lägga till en timmer så att spelet fotgår om någon spelare inte är närvarande
+ göra så att i stället för att stännga ner spelet om någon lämnar så kommer det en bot som tar över denns platts

## Vad som blev bra / inte bra

* #### inte bra
  * för att ta emot kommandon i från klienten eller serverna har jag skrivit en parser funktion och är osäker på om detta är det besta tilvägargångsättet då det blir väldigt o optimera och segt tack vare alla if statment. Men det är ett system som är hyfsat lätt förståligt samt fungerande och lätt att de bugga.
  * nuvarande så går det att koppla upp flera än fyra spelare vilket inte ska vara möjligt. Nuvarande vägrar programmet att startas om man är flera än fyra spelare men det går inte att kicka/lämna spelet utan att allas spel stängs ner.
  
* ### bra
  * i Game classen finns det en parser och en EXE funktion och tack vere dessa funktioner så om jag skulle ändra något prise, namn eller platta över huvud taget så när man startar spelet uppdateras spelet med de nya platt informationen vilket kan leda till en ny spelupplevelse då det kan vara helt nya värden.
  * Jag använder samma system till minna spel kort vilket gör så att man kan ta bort och skapa ny kort.
