-----------------------------[kompilowanie]-----------------------------
1. Stwórz folder projektowy

2. Umieœæ w nim plik programu *.C (main.C np.)

3. Skopiuj do folderu pliki wsadowe:
   a) Kompilator.bat - Kompilator z C do HEX
   b) ClearALL.bat - czyszczenie wszystkich plików lmf, rel, hex, sym, asm, map
   c) ClearDumbFile.bat - czyszczenie plików pozosta³ych po kompilacji lmf, rel, sym, map (wszystkich z tym rozszerzeniem)
   
4. Skopiuj folder startup do swojeg katalogu projektowego (np. Blink\startyp)

<kroki 5..7 tylko przy zmianie mikrokontrolera>

*5. Jeœli piszesz na innego scalaka ni¿ PD78F9116, wykonaj skrypt 

   "\NEC Electronics Tools\CC78K0S\W2.01\src\cc78k0s\bat\mkstup.bat"

   w cmd przejdŸ do podanej lokalizacij i u¿yj polecenia mkstup <nazwauk³adu> np. mkstup f9116

*6. W folderze "NEC Electronics Tools\CC78K0S\W2.01\src\cc78k0s\lib\" znajduj¹ siê potrzebne plik *.rel oraz *.lib
   Skopiuj je do folderu startup z Twojego katalogu projektowego

*7. W razie potrzeby, w folderze "src\cstartn.asm" znajduj¹ siê ich Ÿród³a (ASM)


8. Uruchom skrypt Kompilator.BAT przez dwukrotne klikniêcie go. Automatycznie wykryje pierwszy plik z
rozszerzeniem *.c i podejime kolejjne kroki kompilacji (C->object->linker->objectConverter) Wynik kompilacji:
a) Zielony - nie ma b³êdów, operacja zakoñczona sukcesem
b) Czerwony - wyst¹pi³ b³¹d przy jednym z etapów

-----------------------------[Programowanie MCU]-----------------------------

1. SprawdŸ miernikiem uniwersalnym, czy pin Vpp, #RESET, RX, TX od MCU nie jest zwarty do Vss / Vdd
2. Pod³¹cz programator do mikrokontrolera:

[prog]      [MCU]

[1.Vdd]-----[Vdd/Vcc]
[2.Vpp]-----[Vpp]
[3.RX]------[RX]
[4.TX]------[TX]
[5.#res]----[#reset]
[6.GND]-----[Vss/GND]

**UWAGA** Na piine 2 programatora wystêpuje napiêcie 10V (Vp-p), które mo¿e uszkodziæ uk³ad, jeœli zosta³ Ÿle pod³¹czony!
Nie zgaduj po³¹czeñ, tylko pod³¹cz wed³ug dokumentaji i upewnij siê wielokrotnie, ¿e wszystko zrobi³eœ poprawnie!

3. Pod³¹cz programator do komputera
4. Uruchom aplikacjê NEC_PROG.exe
5. Porgramator powinien zostaæ wykryty automatycznie (w rozwijanym menu pojawi siê jego nazwa i numer seryjny
6. Ustaw iloœæ impulsów na 8 (lub wed³ug dokumentacji dla komunikacji po UART)
7. Kliknij PO£¥CZ - nast¹pi po³¹czenie programatora z komputerem
8. WejdŸ w tryb programowania klikaj¹c przyisk PROG MODE
9. Prgramator spróbuje nawi¹zaæ po³¹czenie z uk³adem przez wys³anie odpowiedniej ilœci impulsów oraz polecenia synchronizacji
10. PrzejdŸ do zak³adki DEVICE. Kliknij przycisk CZYTAJ/READ
11. Jeœli wszystko jest dobrze, program za poœrednictwem programatora odczyta sygnaturê MCU i podzieli j¹ zinterpretuje
12. Program rozpoznaje scalaki PD78F9116, PD78F0134 oraz PD78F0138 (rozwojowe, bêd¹ dodawane). Jeœli twój scalak nale¿y do nich
    to zostanie wyœwietlony jego podgl¹d.
13. W zak³adce FLASH znajduje siê ca³a kontrola nad pamiêci¹ flash MCU

a) LOAD  - za³adowanie pliku z wsadem (*BIN). Program na podstawie rozmiaru pamiêci ROM uk³adu wczyta plik z wsadem
b) ERASE - czyszczenie zawartoœci pamiêci FLASH. Przy ca³kowitym czyszczeniu nale¿y zwiêkszyæ czas kasowania i u¿yæ
   procedury prevrite. (np 15 sekund + prevrite dla F9116)
c) VERIFY - Nie dzia³a *jeszcze*
d) BLANK? - sprawdzenie, czy uk³ad ma wyczyszczon¹ pamiêæ FLASH
e) PROG - Programowanie uk³adu na podstawie za³adowanego bufora (przycisk LOAD)

Przyk³adowa procedura
1) [BLANK] jeœli czysty, przejdŸ do kroku x
2) w przeciwnym razie, zwiêksz czas erase time (+/-10s), u¿yj opcji prewrite, [ERASE]
   jeœli b³¹d kasowania, odznacz prewrite (operacja jednorazowa), zwiêksz czas o kilka sekund i ponów kasowanie
3) [BLANK] - uk³ad powinien byæ ju¿ czysty. Jeœli nie, to œmieæ/uszkodzony
4) [LOAD] - za³aduj plik binarny, którym chcesz zaprogramowaæ uk³ad.
   Jeœli program NEC_PROG.exe nie otwiera jeszcze plików HEX, to uzyskany z kompilacji plik HEX nale¿y otworzyæ
   w dowolnym edytorze HEX i zapisaæ go jako plik BIN (np. BLINK.HEX na BLINK.BIN)
   Program narysuje mapê pamiêci i ewentualnie j¹ pokoloruje wg ustawieñ. Mapa s³u¿y wy³¹cznie jako podgl¹d.
5) Zmniejsz czas "erase time" na minimum, przyspieszy to proces programowania
6) [PROG], zatwierdŸ komunikat - [YES]
7) Po skoñczonym programowaniu zostanie wys³any komunikat
8) Jeœli weryfikacja zostanie naprawiona, u¿yj jej - [VERIFY]

   