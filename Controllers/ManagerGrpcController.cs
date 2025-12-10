using Grpc.Net.Client;
using ManagerService; // Namespace-ul serviciului gRPC (ManagerService)
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
// Presupunem că modelul Manager este accesibil aici (Movies_Project.Models.Manager)
using Manager = ManagerService.Manager; 

namespace Movies_Project.Controllers
{
    public class ManagersGrpcController : Controller
    {
        private readonly GrpcChannel channel;

        public ManagersGrpcController()
        {
            // ATENȚIE: Verifică portul corect! 
            // Acesta trebuie să fie portul HTTPS la care rulează serviciul tău gRPC (ManagerService)
            channel = GrpcChannel.ForAddress("https://localhost:5002"); // Portul gRPC Server (ex: 5002)
        }

        // GET: ManagersGrpc/Index
        [HttpGet]
        public IActionResult Index()
        {
            var client = new ManagerCRUDService.ManagerCRUDServiceClient(channel);
            
            // Apel gRPC: GetAll
            ManagerList managerList = client.GetAll(new Empty());
            
            // Trimite lista de manageri către View
            return View(managerList);
        }

        // GET: ManagersGrpc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ManagersGrpc/Create
        [HttpPost]
        public IActionResult Create(Manager manager)
        {
            // Validarea modelului gRPC nu se face direct, dar verificăm dacă datele sunt prezente
            if (manager != null) 
            {
                var client = new ManagerCRUDService.ManagerCRUDServiceClient(channel);
                
                // Apel gRPC: Insert
                client.Insert(manager);
                
                // Redirecționează către lista de manageri
                return RedirectToAction(nameof(Index));
            }
            
            // Dacă modelul e invalid sau lipsește
            return View(manager);
        }
    }
}