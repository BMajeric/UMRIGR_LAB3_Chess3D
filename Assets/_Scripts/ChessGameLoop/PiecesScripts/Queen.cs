namespace ChessMainLoop
{
    public class Queen : Piece
    {    
        public override void CreatePath()
        {
            /*
             * Nadopunite kod za stvaranje objekata za odabir polja koji prati logiku figure kraljice.
             */
        }

        public override bool IsAttackingKing(int row, int column)
        {
            /*
             * Zamijenite liniju return false; sa kodom za provjeru napada li kraljica kralja sa trenutnog polja.
            */
            return false;
        }

        public override bool CanMove(int row, int column)
        {
            /*
             * Zamijenite liniju return false; sa kodom za provjeru ima li kraljica preostalih dopuštenih poteza.
            */
            return false;
        }
    }
}