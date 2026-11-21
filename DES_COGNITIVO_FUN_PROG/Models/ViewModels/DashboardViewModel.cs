namespace DES_COGNITIVO_FUN_PROG.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalClientesActivos { get; set; }
        public int TotalEventosActivos { get; set; }
        public int TotalInvitados { get; set; }
        public int EventosEsteMes { get; set; }

        // lista de columnas del tablero de estados, una columna por cada estado de evento
        public List<ColumnaEstadoViewModel> ColumnasEstados { get; set; } = new List<ColumnaEstadoViewModel>();
    }

    // representa una columna del tablero del dashboard (un estado y los eventos que estan en ese estado)
    public class ColumnaEstadoViewModel
    {
        public int IdEstado { get; set; }
        public string NombreEstado { get; set; } = null!;
        public List<EventoTableroViewModel> Eventos { get; set; } = new List<EventoTableroViewModel>();
    }

    // datos resumidos de un evento para mostrarlo como tarjeta dentro del tablero
    public class EventoTableroViewModel
    {
        public int IdEvento { get; set; }
        public int IdEstado { get; set; }
        public string CodigoEvento { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public DateOnly Fecha { get; set; }
        public string Lugar { get; set; } = null!;
        public string NombreCliente { get; set; } = null!;
        public int TotalInvitados { get; set; }
        public int TotalConfirmados { get; set; }
    }
}