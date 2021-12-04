var input = File.ReadAllLines("input");
var randomDraws = input[0].Split(',').Select(s => int.Parse(s)).ToArray();

var boards = Board.CreateBoards(input.Skip(2).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray());

var bingoGame = new Bingo(randomDraws, boards);
bingoGame.Play();

Console.WriteLine($"Number of winner = {bingoGame.WinnerBoards?.Count}, Board number = {bingoGame.WinnerBoards?[0].BoardNumber}, final score = {bingoGame.WinnerBoards?[0].FinalScore()}");

class Board {

    public List<int> Grid { get; }
    public List<bool> DrawnGrid { get; }

    public int GridSize { get; }

    public int BoardNumber { get; }

    public bool HasWon { get; private set; }

    public List<int> DrawnNumbersHistory { get; private set; } = new List<int>();

    private Board(List<int> grid, int boardNumber, int gridSize) {
        Grid = grid;
        BoardNumber = boardNumber;
        GridSize = gridSize;
        DrawnGrid = Enumerable.Repeat(false, gridSize * gridSize).ToList();
    }

    public void ParticipateInBingo(Bingo bingo) {
        bingo.NewRandomNumberIsDrawn += HandleNewRandomNumberDrawn;
    }

    private void HandleNewRandomNumberDrawn(object? sender, int number) {
        DrawnNumbersHistory.Add(number);
        var index = Grid.FindIndex(n => n == number);
        if (index >= 0) {
            DrawnGrid[index] = true;
        }
        CheckHasWon();
    }

    private void CheckHasWon() {
        for (int i = 0; i < GridSize; i++) {
            if (DrawnGrid.Skip(GridSize * i).Take(GridSize).All(b => b) || DrawnGrid.Where((n, index) => index % GridSize == i).All(b => b)) {
                HasWon = true;
                break;
            }
        }
    }

    public int Score() {
        return Grid.Where((number, index) => !DrawnGrid[index]).Sum();
    }

    public int FinalScore() {
        return DrawnNumbersHistory.Last() * Score();
    }

    // Create boards from our input format
    public static List<Board> CreateBoards(string[] input) {
        var boardsInput = input;
        var gridSize = boardsInput[0].Split(' ').Where(s => !string.IsNullOrEmpty(s)).Select(s => int.Parse(s)).Count();

        if (input.Length % gridSize != 0) {
            throw new ArgumentException("Bad input, grid size should be constant for all boards and all boards should be completly defined");
        }

        var output = new List<Board>();
        while (boardsInput.Length != 0) {
            var grid = string.Join(' ', boardsInput.Take(gridSize)).Split(' ').Where(s => !string.IsNullOrEmpty(s)).Select(s => int.Parse(s)).ToList();

            if (grid.Count != gridSize * gridSize) {
                throw new ArgumentException($"Bad input, grid size for board {output.Count} should be {gridSize} x {gridSize} and it is {grid.Count}");
            }

            output.Add(new Board(grid, output.Count, gridSize));
            boardsInput = boardsInput.Skip(gridSize).ToArray();
        }
        return output;
    }
}

class Bingo {

    public int[] RandomDraws { get; }

    public List<Board> Boards { get; }

    public List<Board>? WinnerBoards { get; private set; }

    public event EventHandler<int>? NewRandomNumberIsDrawn;

    public Bingo(int[] randomDraws, List<Board> boards) {
        RandomDraws = randomDraws;
        Boards = boards;
        foreach (var board in Boards) {
            board.ParticipateInBingo(this);
        }
    }

    public void Play() {
        foreach (var draw in RandomDraws) {
            NewRandomNumberIsDrawn?.Invoke(this, draw);
            WinnerBoards = Boards.Where(b => b.HasWon).ToList();
            if (WinnerBoards.Count > 0) {
                break;
            }
        }
    }
}
