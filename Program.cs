using Electre;
using Electre.src;
using Electre.src.Data;

Console.WriteLine("Eletre Console Application\n");

string path = "test.txt";

List<string> criterionNames;
List<int> criterionWeights;
List<Criterion> criterions = new List<Criterion>();
List<Value> values = new List<Value>();

using (StreamReader reader = new StreamReader(path))
{
    string? line;

    line = reader.ReadLine();

    criterionNames = line.Split(" ").ToList();

    line = reader.ReadLine();

    criterionWeights = line.Split(" ").Select(x => Int32.Parse(x)).ToList();

    for (int i = 0; i < criterionNames.Count; i++)
    {
        criterions.Add(new Criterion() { Name = criterionNames[i], Weight = criterionWeights[i] });
    }

    while ((line = reader.ReadLine()) != null)
    {
        string[] option = line.Split(" ");

        string optionName = option[0];

        for (int i = 1; i < option.Length; i++)
        {
            var index = new Value()
            {
                Val = int.Parse(option[i]),
                Option = optionName,
                Criterion = criterions[i - 1]
            };

            values.Add(index);
        }
    }
}

var printer = new MyElectrePrinter();

var emh = new ElectreMethodHandler(printer, criterions, values);

var res = emh.Handle(true);