//$n ZWIERZYNIEC - ED72 6:01
//$d
//$d Autor trasy: Wasyl, autor scenariusza: Woku, modyfikacje: HTD
//$d
//$d Prowadzimy osobowy (EZT) 65113 relacji Kociary - Pawianowo, planowy odjazd 6:01. Czas misji: oko³o 1h
//$d
//$d Scenariusz z interaktywn¹ prób¹ hamulca i dynamicznym rozk³adem jazdy.

//$i zwierzyniec_ed72.jpg
//$f pl scenery/zwierzyniec/htd/roj65113.txt Rozk³ad jazdy
//$f pl inne/zwierzyniec/htd/index.html Opis misji

time 05:45 6:53 16:24 endtime

sky cgskj_dusk006.t3d endsky
atmo 0 0 0 800 2000 0.067 0.078 0.09 endatmo
light -1 -1 -2 0.11 0.102 0.078 0.157 0.131 0.145 0.173 0.173 0.173 endlight
config movelight 1 doubleambient no endconfig // jedyna konfiguracja z movelight gdzie to dobrze wygl¹da

include zwierzyniec/htd/tory.scm end
include zwierzyniec/tlk/drogi.scm end
include zwierzyniec/htd/przejazdy.scm end
include zwierzyniec/tlk/lasy.scm end
include zwierzyniec/tlk/sygnalizacja.scm end
include zwierzyniec/tlk/teren.scm end
include zwierzyniec/htd/wskazniki.scm end
include zwierzyniec/tlk/trakcja.scm end
include zwierzyniec/tlk/trawa.scm end
include zwierzyniec/tlk/reszta.inc end
include zwierzyniec/htd/posers.inc end
include zwierzyniec/htd/zdarzenia.ctr end

FirstInit

trainset zwierzyniec/htd/roj65113 tor_trasa_a_start_lok 10.0 0.0
//$o Rozpoczynamy na bocznicy stacji Kociary. Stoimy w grupie postojowej na torze z ograniczeniem prêdkoœci do 5km/h. Po uruchomieniu jednostki nale¿y podjechaæ do najbli¿szej tarczy manewrowej, nastêpnie podane zostan¹ manewry, które maj¹ na celu ustawienie jednostki "w peronach". Gdy ju¿ stoimy przy peronie, nale¿y zmieniæ kabinê. Po tym, zgodnie z komendami podawanymi przez radiotelefon, wykonaæ nale¿y próbê hamulca. Gdy podany zostanie sygna³ na semaforze wyjazdowym, ruszamy jako poci¹g osobowy relacji Kociary - Pawianowo. Na stacjach i przystankach osobowych nale¿y oczekiwaæ z zamkniêciem drzwi do czasu podania sygna³u "Odjazd" od kierownika poci¹gu przez radiotelefon. Po dotarciu do stacji koñcowej nale¿y ponownie zmieniæ kabinê aby zjechaæ jednostk¹ w tory postojowe (tzw. ¿eberko).
//$w type=p,lmax=319,vmin=60,vmax=120,d=1.435
node -1 0 ED72-012-RA dynamic PKP\ED72_V1 ED72-012-RA 5BS-RA 0 headdriver 63 29 Passengers enddynamic
node -1 0 ED72-010sa dynamic PKP\ED72_V1 ED72-012-SA 6BS-SA 0 nobody 63 31 Passengers enddynamic
node -1 0 ED72-010sb dynamic PKP\ED72_V1 ED72-012-SB 6BS-SB 0 nobody 63 30 Passengers enddynamic
node -1 0 ED72-010rb dynamic PKP\ED72_V1 ED72-012-RB 5BS-RB 0 nobody 0 28 Passengers enddynamic
endtrainset

include zwierzyniec/htd/sklady.inc end