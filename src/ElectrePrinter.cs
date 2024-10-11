using Electre.src.Data;
using Index = Electre.src.Data.Index;

namespace Electre.src
{
    public abstract class ElectrePrinter
    {
        internal abstract void SetPrint(bool print = true);

        internal abstract void PrintComparedPairs(List<ComparedPair> comparedPairs, List<Criterion> criterions);

        internal abstract void PrintIndexTable(List<Index> table, string title);

        internal abstract void PrintResultSet(List<Result> resultSet);
    }
}
