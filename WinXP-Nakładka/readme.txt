-----------------------------[kompilowanie]-----------------------------
1. Stw�rz folder projektowy

2. Umie�� w nim plik programu *.C (main.C np.)

3. Skopiuj do folderu pliki wsadowe:
   a) Kompilator.bat - Kompilator z C do HEX
   b) ClearALL.bat - czyszczenie wszystkich plik�w lmf, rel, hex, sym, asm, map
   c) ClearDumbFile.bat - czyszczenie plik�w pozosta�ych po kompilacji lmf, rel, sym, map (wszystkich z tym rozszerzeniem)
   
4. Skopiuj folder startup do swojeg katalogu projektowego (np. Blink\startyp)

<kroki 5..7 tylko przy zmianie mikrokontrolera>

*5. Je�li piszesz na innego scalaka ni� PD78F9116, wykonaj skrypt 

   "\NEC Electronics Tools\CC78K0S\W2.01\src\cc78k0s\bat\mkstup.bat"

   w cmd przejd� do podanej lokalizacij i u�yj polecenia mkstup <nazwauk�adu> np. mkstup f9116

*6. W folderze "NEC Electronics Tools\CC78K0S\W2.01\src\cc78k0s\lib\" znajduj� si� potrzebne plik *.rel oraz *.lib
   Skopiuj je do folderu startup z Twojego katalogu projektowego

*7. W razie potrzeby, w folderze "src\cstartn.asm" znajduj� si� ich �r�d�a (ASM)


8. Uruchom skrypt Kompilator.BAT przez dwukrotne klikni�cie go. Automatycznie wykryje pierwszy plik z
rozszerzeniem *.c i podejime kolejjne kroki kompilacji (C->object->linker->objectConverter) Wynik kompilacji:
a) Zielony - nie ma b��d�w, operacja zako�czona sukcesem
b) Czerwony - wyst�pi� b��d przy jednym z etap�w

-----------------------------[Programowanie MCU]-----------------------------

1. Sprawd� miernikiem uniwersalnym, czy pin Vpp, #RESET, RX, TX od MCU nie jest zwarty do Vss / Vdd
2. Pod��cz programator do mikrokontrolera:

[prog]      [MCU]

[1.Vdd]-----[Vdd/Vcc]
[2.Vpp]-----[Vpp]
[3.RX]------[RX]
[4.TX]------[TX]
[5.#res]----[#reset]
[6.GND]-----[Vss/GND]

**UWAGA** Na piine 2 programatora wyst�puje napi�cie 10V (Vp-p), kt�re mo�e uszkodzi� uk�ad, je�li zosta� �le pod��czony!
Nie zgaduj po��cze�, tylko pod��cz wed�ug dokumentaji i upewnij si� wielokrotnie, �e wszystko zrobi�e� poprawnie!

3. Pod��cz programator do komputera
4. Uruchom aplikacj� NEC_PROG.exe
5. Porgramator powinien zosta� wykryty automatycznie (w rozwijanym menu pojawi si� jego nazwa i numer seryjny
6. Ustaw ilo�� impuls�w na 8 (lub wed�ug dokumentacji dla komunikacji po UART)
7. Kliknij PO��CZ - nast�pi po��czenie programatora z komputerem
8. Wejd� w tryb programowania klikaj�c przyisk PROG MODE
9. Prgramator spr�buje nawi�za� po��czenie z uk�adem przez wys�anie odpowiedniej il�ci impuls�w oraz polecenia synchronizacji
10. Przejd� do zak�adki DEVICE. Kliknij przycisk CZYTAJ/READ
11. Je�li wszystko jest dobrze, program za po�rednictwem programatora odczyta sygnatur� MCU i podzieli j� zinterpretuje
12. Program rozpoznaje scalaki PD78F9116, PD78F0134 oraz PD78F0138 (rozwojowe, b�d� dodawane). Je�li tw�j scalak nale�y do nich
    to zostanie wy�wietlony jego podgl�d.
13. W zak�adce FLASH znajduje si� ca�a kontrola nad pami�ci� flash MCU

a) LOAD  - za�adowanie pliku z wsadem (*BIN). Program na podstawie rozmiaru pami�ci ROM uk�adu wczyta plik z wsadem
b) ERASE - czyszczenie zawarto�ci pami�ci FLASH. Przy ca�kowitym czyszczeniu nale�y zwi�kszy� czas kasowania i u�y�
   procedury prevrite. (np 15 sekund + prevrite dla F9116)
c) VERIFY - Nie dzia�a *jeszcze*
d) BLANK? - sprawdzenie, czy uk�ad ma wyczyszczon� pami�� FLASH
e) PROG - Programowanie uk�adu na podstawie za�adowanego bufora (przycisk LOAD)

Przyk�adowa procedura
1) [BLANK] je�li czysty, przejd� do kroku x
2) w przeciwnym razie, zwi�ksz czas erase time (+/-10s), u�yj opcji prewrite, [ERASE]
   je�li b��d kasowania, odznacz prewrite (operacja jednorazowa), zwi�ksz czas o kilka sekund i pon�w kasowanie
3) [BLANK] - uk�ad powinien by� ju� czysty. Je�li nie, to �mie�/uszkodzony
4) [LOAD] - za�aduj plik binarny, kt�rym chcesz zaprogramowa� uk�ad.
   Je�li program NEC_PROG.exe nie otwiera jeszcze plik�w HEX, to uzyskany z kompilacji plik HEX nale�y otworzy�
   w dowolnym edytorze HEX i zapisa� go jako plik BIN (np. BLINK.HEX na BLINK.BIN)
   Program narysuje map� pami�ci i ewentualnie j� pokoloruje wg ustawie�. Mapa s�u�y wy��cznie jako podgl�d.
5) Zmniejsz czas "erase time" na minimum, przyspieszy to proces programowania
6) [PROG], zatwierd� komunikat - [YES]
7) Po sko�czonym programowaniu zostanie wys�any komunikat
8) Je�li weryfikacja zostanie naprawiona, u�yj jej - [VERIFY]

   