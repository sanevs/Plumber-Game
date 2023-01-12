using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;

namespace ClientPlumber
{
    public class CellCollection : BindableBase
    {
        private IList<Cell> cells;
        public IList<Cell> Cells
        {
            get => cells;
            set
            {
                if (cells == value)
                    return;
                SetProperty(ref cells, value);
            }
        }

        private IList<Cell> enemyCells;

        public IList<Cell> EnemyCells
        {
            get => enemyCells;
            set
            {
                if (enemyCells == value)
                    return;
                SetProperty(ref enemyCells, value);
            }
        }

        public CellCollection()
        {
            Cells = Create();
            EnemyCells = Create();
        }
        private IList<Cell> Create() => new List<Cell>(Enumerable
            .Range(0, 25)
            .Select(i => new Cell()));
    }
}
