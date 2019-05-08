
<div style="border: solid 1px red">
  <h1>monopol</h1>
  Detta är mitt slutprodject till programmering 2
  Det är en monopol variant med alla regler som jag han lägga till
</div>

## **Regler**/**Funktioner**
+ Kasta tärningar
+ liten pjäs som flytar efter pixlar
+ möjligheten att ändra reglerna på spelplanen utan att kompalera om programmet med hjälp av spelets inbygda **ConfParser** tack vare den kan man ändra i filen **Plate.conf** och få en helt ny spelplan upplevelse med nya placeringar av hus samt nya priser/ namn och händelser
+ samma system används till korten vilket gör så att man kan skriva om alla korten och ändra kotens funktioner


## Försat planering och nu

Följde min plan ty helt.

Det enda jag märkte när jag började programmera var att jag hade tänkt helt fel på hur min sörver skulle fungera,
jag började med att göra så att en dator var sörver och alla andra datorer var klienter. Detta lede till att jag var tvunngen att behandla klienten
och servern olika tach vare att de gjorde olika saker och server var tvungen att kunna göra samma sak som klienten och samtidigt göra det som sörven ska gör(skucka runt information).
detta gjorde det väldigt svårt för mig att tänka hur programmet skulel fungerade. Jag valde därför att göra en dator till både server och kient samtidig fast driva dem på varsin tråd.

## Vad jag hade viljat lägga till(kanske komemr lägga till unde sommarlåvet)
* möligheten att sälja plattor till andra spelare
* möjligheten att köra över nätverk
* Vissa delar i programmet är väldigt dåligt optimerade vilket gör programmet segt då och då
* skriva ut namn och pris för varje platta på skärmen
