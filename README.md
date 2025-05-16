#Месенджер

**Шевчук Владислав ІПЗ-23-1**

##Запуск проєкту

Спершу необхідно запустити, сервер виконавши команду ```dotnet run``` у папці з проєктом ChatServer
Запустити клієнтський додаток, виконавши команду ```dotnet run``` у папці з проєктом ChatApp

##Функціональність

*Головне вікно клієнтського додатку
  *Два представлення: Авторизація та вікно чату
    *Меню авторизації: вкладки логін та реєстрація
      *Виведення інформації про статус реєстрації, авторизації та з'єднання
    *Вікно чату:
      *Бокова панель з користувачами онлайн
      *Рядок відправки повідомлень
      *Чат з повідомленнями
*Консоль серверу
  *Логування інформації про операції на сервері та його клієнтів
*Основний функціонал:
  *Реєстрація нових користувачів
  *Авторизація вже існуючих
  *Відправка та отримання повідомлень
  *Історія повідомлень
  *Збереження користувачів та їх повідомлень у базі даних

##Programming Principles

###DRY
Для уникать повторення коду я інкапсулював логіку,яка має повторюватися багато разів, у окремі методи чи класи.
Метод для створення моделі повідомлення [CreateMessageModel()](ChatApp/Core/Net/CreateMessageModel.cs#L97-L125) допомагає уникнути повторень.
###KISS
Проста та зрозуміла структура класів з інтуїтивними назвами.
Клас [MessageService](ChatApp/Core/Services/MessageService.cs) має простий інтерфейс з методами AddMessage(), GetMessages() та ClearMessages() без зайвої складності.
###SOLID
####Single Responsibility Principle
Кожен клас має єдину причину для змін та одну відповідальність, наприклад:
*[PacketReader](ChatServer/Core/Net/IO/PacketReader.cs) - відповідає лише за читання пакетів даних з мережевого потоку
*[PacketBuilder](ChatServer/Core/Net/IO/PacketBuilder.cs) - відповідає лише за створення пакетів даних для передачі
*[ClientOperations](ChatServer/Core/Net/ClientOperations/) - кожна операція ([Login](ChatServer/Core/Net/ClientOperations/LoginOperation.cs), [Register](ChatServer/Core/Net/ClientOperations/RegisterOperation.cs), [Message](ChatServer/Core/Net/ClientOperations/MessageOperation.cs) тощо) виділена в окремий клас з власною відповідальністю
####Open/Closed Principle
Система пакетних обробників відкрита для розширення через інтерфейс [IPacketHandler](ChatApp/Core/Net/Handlers/PacketHandler.cs).
Можна додати новий тип пакету, створивши новий клас-обробник, який реалізує [IPacketHandler](ChatApp/Core/Net/Handlers/PacketHandler.cs), без зміни існуючого коду в [PacketHandlerFactory](ChatApp/Core/Net/Handlers/PacketHandlerFactory.cs).
####Liskov Substitution Principle
Об'єкти в програмі можуть бути замінені їх підтипами без порушення правильності роботи програми, наприклад:
*[ServerBase](ChatServer/Core/Net/ServerBase.cs) і [Server](ChatServer/Core/Net/Server.cs) - базовий абстрактний клас та його реалізація
*[IClient](ChatServer/Core/Interfaces/IClient.cs) та [Client](ChatServer/Core/Net/Client.cs) - інтерфейс та його реалізація
####Interface Segregation Principle
Клієнти не повинні залежати від інтерфейсів, які вони не використовують. Наприклад [IAuthService](ChatApp/Core/Services/Interfaces/IAuthService.cs), [IMessageService](ChatApp/Core/Services/Interfaces/IMessageService.cs), [IUserService](ChatApp/Core/Services/Interfaces/IUserService.cs) - містять тільки методи необхідні для специфікації роботи сервісів, які мають бути реалізовані на їх основі.
####Dependency Inversion Principle
Залежності від абстракцій, а не конкретних класів, наприклад:
*[ChatViewModel](ChatApp/MVVM/ViewModel/ChatViewModel.cs#L40) - залежить від інтерфейсів IServerConnection, IMessageService, IUserService, IAuthService, а не від їх конкретних реалізацій.
*[MessageHandlerService](ChatServer/Core/Services/MessageHandlerService.cs#L12) - залежить від інтерфейсів IMessageRepository, IUserRepository, IClientManager.
###Fail Fast
Раннє виявлення та обробка помилок. 
В методах [SendMessageAsync](ChatApp/Core/Net/ServerConnection.cs#L213-L232), [ConnectAsync](ChatApp/Core/Net/ServerConnection.cs#L127-L146), [UseExistingConnectionAsync](ChatApp/Core/Net/ServerConnection.cs#L148-L170) класу [ServerConnection](ChatApp/Core/Net/ServerConnection.cs) перевіряється стан з'єднання і авторизації перед спробою відправки повідомлень чи спробою встановлення з'єднання з сервером.

##Design Patterns

###MVVM (Model-View-ViewModel)
Полегшує відокремлення розробки графічного інтерфейсу від розробки бізнес логіки. Використовується для відокремлення моделі та її відображення.
UI клієнтського додатку реалізовано з використанням цього патерну: через модель вигляду [ChatViewModel](ChatApp/MVVM/ViewModel/ChatViewModel.cs) модель [MessageModel](ChatApp/MVVM/Model/MessageModel.cs) відображається і взаємодіє з виглядом [ChatView](ChatApp/MVVM/View/ChatView.xaml)
###Command
Інкапсуляція запиту як об'єкту. [RelayCommand](ChatApp/Core/RelayCommand.cs) реалізує інтерфейс ICommand для виконання дій з UI (використовується наприклад для створення реакції на натисканная кнопки, яка виконує [реєстрацію](ChatApp/MVVM/ViewModel/LoginViewModel.cs#L62) або [логін](ChatApp/MVVM/ViewModel/LoginViewModel.cs#L56) користувача).
###Strategy
Патерн, який створює сімейство алгоритмів, інкапсулює кожен з них і забезпечує їх взаємозамінність. Допомагає додавати необхідні нові дії чи операції без зміни існуючого коду.
*На основі інтерфейсу [IPacketHandler](ChatApp/Core/Net/Handlers/IPacketHandler.cs) створено багато різних обробників пакетів.
*На основі інтерфейсу [IClientOperation](ChatServer/Core/Net/ClientOperations/Interfaces/IClientOperation.cs) створено багато різних стратегій обробки операцій з клієнтами.
###Template
Паттерн, що визначає скелет алгоритму, перекладаючи деякі кроки на підкласи.
[ServerBase](ChatServer/Core/Net/ServerBase.cs) - абстрактний клас, що визначає шаблон роботи сервера
[Server](ChatServer/Core/Net/Server.cs - конкретна реалізація, що перевизначає абстрактні методи
###Dependency Injection
Паттерн, що допомагає у передачі залежностей об'єкту ззовні замість створння їх всередині об'єкта. 
*[ServiceCollectionExtensions](ChatApp/Extensions/ServiceCollectionExtensions.cs) - реєстрація всіх залежностей клієнтського додатку
*[ServiceCollectionExtensions](ChatServer/Extensions/ServiceCollectionExtensions.cs) - реєстрація всіх залежностей серверного додатку
###Factory
*[PacketHandlerFactory](ChatApp/Core/Net/Handlers/PacketHandlerFactory.cs) - фабрика, що допомагає реагувати на різні OpCode, створюючи та повертаючи обробники пакетів.
*[ClientOperationFactory](ChatServer/Core/Net/ClientOperations/ClientOperationFactory.cs) - фабрика, що повертає відповідну від ситуації стратегію обробки клієнтських операцій

##Refactoring Techniques

###Extract Method
Виділення фрагментів логіки з одного супер методу в різні і їх подальший виклик у першому, для полегшення читабельності коду. Наприклад:
У класі [ServerConnection](ChatApp/Core/Net/ServerConnection.cs) логіка реєстрації обробників винесена в окремі методи [RegisterConnectedHandler()](ChatApp/Core/Net/ServerConnection.cs#L47-L56), [RegisterMessageHandler()](ChatApp/Core/Net/ServerConnection.cs#L58-L67) і тд. Ці методи викликаються в колишньому супер методі [RegisterHandlers()](ChatApp/Core/Net/ServerConnection.cs#L39-L45)
###Extract Class
Виділення частини логіки класу(методів та полів), у нові класи для покрашення читабельності та повторного використання в інших класах. Наприклад, у моєму проєкті з використанням патерну стратегія було виділено операції клієнтів [IPacketHandler](ChatApp/Core/Net/Handlers/IPacketHandler.cs) та стратегії обробки операцій з клієнтами [IClientOperation](ChatServer/Core/Net/ClientOperations/Interfaces/IClientOperation.cs) з інших класів. 
###Decompose Conditional
В методі [CreateMessageModel()](ChatApp/Core/Net/ServerConnection.cs#L97-L125) класу [ServerConnection](ChatApp/Core/Net/ServerConnection.cs) виділяється обробка складної умови для парсингу повідомлення, що необхідна для роботи кілької інших методів.
###Replace Conditional with Polymorphism
[ClientOperationFactory](ChatServer/Core/Net/ClientOperations/ClientOperationFactory.cs) - використовується замість великої структрури switch/case для обробки різних операцій використовується словник і поліморфізм через інтерфейс [IClientOperation](ChatServer/Core/Net/ClientOperations/Interfaces/IClientOperation.cs)
###Replace Magic Number with Symbolic Constant
Заміна магічних чисел у клієнті [OpCodes](ChatApp/Constants/OpCodes.cs) та сервері [OpCodes](ChatServer/Constants/OpCodes.cs), що використовуються для визначення кодів операцій по всьому проєкту. Це допомогає не заплутатися, за що який код відповідає.
###Introduce Parameter Object
Групування параметрів у об'єкти для легшої взаємодії та передачі значень.
Використання моделей [MessageModel](ChatApp/MVVM/Model/MessageModel.cs) та [UserModel](ChatApp/MVVM/Model/UserModel.cs) замість окремих параметрів. Наприклад у методі [CreateMessageModel()](ChatApp/Core/Net/ServerConnection.cs#L97-L125)

##Скриншоти
![Login Menu](/Screenshots/image_2025-05-16_17-02-40.png)
![Register Menu and Error info](/Screenshots/image_2025-05-16_17-03-10.png)
![Chat](/Screenshots/image_2025-05-16_17-04-08.png)
![Two users online](/Screenshots/image_2025-05-16_17-10-04.png)
![Console log output](/Screenshots/image_2025-05-16_17-13-20.png)
