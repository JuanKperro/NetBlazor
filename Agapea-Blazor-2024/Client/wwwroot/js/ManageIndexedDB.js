//fichero javascript que define un objeto que cuelga directamente de "window" (para hacerlo global a todos los comp.blazor)
//con props y metodos para manejar datos usando indexedDB del navegador

/*  cada op.sobre una BD indexedDB es un objeto implementa IDBRequest, susceptible de eventos "success", "error"
    dominio ----> conjunto de BD ----> BD (nombre,version) <------ objectStore-1 <== datos: { key, value } (index busqueda)
                                                                   objectStore-2 <== datos: { key, value } (index busqueda)
                                                                   ....
                                                                   ------toda op. objectStore: necesita Transaccion--------------
  cuando abres conexion a una BD, se lanza evento especial a parte del sucess, y el error:  upgradeneedeed
  se produce siempre cuando:
        - intentas abrir una BD q no existe (se crea por primera vez) <===== creas objectstores, indices, etc
        - o intentas abrir una VERSION de la BD superior a la q esta creada

    agapea2024:

        objectstore  clienteLogueado
        ---------------------------
        { 'clienteLoggedIn', 'mio@mio.es' } <------ solo un documento!!! cliente logueado...(se hace para evitar modificar interface IStorageService)
                                     para evitar crear metodos q lleven el email del cliente y tengas q refactorizar todos los
                                     servicios ya creados q usan esa interface (LocalStorageService, ...)
        objectSTore datosclientes
        -------------------------
        { 'mio@mio.es', { nombre:'', apellidos:'', .....} }
        { 'otro@otro.com', { nombre:'', apellidos:'', .....} }
        ...
        objectstore tokens
        ------------------
        { 'mio@mio.es', 'ffafasdfsafsafsasdfasdfsafd.fasfsadfsafsdafsdaf...'}
        { 'otro@otro.com', 'ffafasdfsafsafsasdfasdfsafd.fasfsadfsafsdafsdaf...'}
        ...


*/


//como las op.son asincronas, cuando  finalice la op.sobre la api del navegador, debemos llamar a metodos de .net
//servicio, componente, etc
//https://learn.microsoft.com/es-es/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-8.0
//basicamente consiste en pasar una ref. del servicio, componente, etc...a los metodos javascript desde los q te interese
//invoca a metodos .net, y con esa ref. invocar dichos metodos
//esta ref. se define en el servicio o comp. objeto clase: DotNetObjectReference<clase_servicio>
//a los metodos .NET invocables desde js, se les pone atributo: JSInvocable
function _crearEsquemaIndexedDB(ev) {
    var _bd = ev.target.result; //<---- objeto IDBDatabase

    var _clientesObjectStore = _bd.createObjectStore('datosclientes', { keypath: 'email' }); //<--- objeto IDBObjectstore
    _clientesObjectStore.createIndex('email', 'email', { unique: true });

    var _tokensObjectStore = _bd.createObjectStore('tokens', { keypath: 'email' }); //<--- objeto IDBObjectstore
    _tokensObjectStore.createIndex('email', 'email', { unique: true });

    var _clienteLoguedObjectStore = _bd.createObjectStore('clienteLogueado', { keypath: 'email' }); //<--- objeto IDBObjectstore
    _clienteLoguedObjectStore.createIndex('email', 'email', { unique: true });


}


window.adminIndexedDB = {
    almacenarDatosCliente: (datoscliente) => {
        var _reqDB = indexedDB.open('agapea2024', 1);

        _reqDB.addEventListener('upgradeneeded', (ev) => _crearEsquemaIndexedDB(ev));
        _reqDB.addEventListener('error', (error) => console.log('error al abrir bd agepa2024 de indexedDB....', error));
        _reqDB.addEventListener('success', (ev) => {
            //para almacenar datos asociados a un cliente, en objectStore 'datoscliente'
            //me creo objeto IDBTransaction de tipo readwrite
            var _db = ev.target.result;
            var _transacDatosCliente = _db.transaction(['datosclientes','clienteLogueado'], 'readwrite');

            //1º op.transaccion almacenamiento datos en 'datoscliente'
            var _reqInsertDatosCliente = _transacDatosCliente.objectStore('datosclientes').add(datoscliente, datoscliente.credenciales.email);
            _reqInsertDatosCliente.addEventListener('success', (evt) => {
                var _datosCliente = evt.target.result;
                console.log('datos insertados de forma correcta en el objecstore datosclientes...', _datosCliente);
            });
            _reqInsertDatosCliente.addEventListener('error', (err) => { console.log(`error al recuperar datoscliente con emnail: ${datos.credenciales.email}...`, err); _transacDatosCliente.abort(); });

            //2º op.transaccion almacenamiento datos en 'clienteLogueado'
            var _reqInsertClienteLogueado = _transacDatosCliente.objectStore('clienteLogueado').add(datoscliente.credenciales.email, 'clienteLoggedIn');
            _reqInsertClienteLogueado.addEventListener('success', () => console.log('email almacenado oks en clienteLogueado'));
            _reqInsertClienteLogueado.addEventListener('error', () => { console.log('error email almacenado en clienteLogueado'); _transacDatosCliente.abort(); });

        });

    },
    recuperarDatosCliente: (refServicioNET,email) => {
        var _reqDB = indexedDB.open('agapea2024', 1);

        _reqDB.addEventListener('error', (error) => console.log('error al abrir bd agepa2024 de indexedDB....', error));
        _reqDB.addEventListener('upgradeneeded', (ev) => _crearEsquemaIndexedDB(ev)); 
        _reqDB.addEventListener('success', (ev) => {
            //para recuperar datos asociados a un cliente, del objectStore 'datoscliente'
            //me creo objeto IDBTransaction de tipo readonly
            var _db = ev.target.result;

            var _transac = _db.transaction(['datosclientes', 'clienteLogueado'], 'readonly');

            var _reqSelectClienteLog = _transac.objectStore('clienteLogueado').get('clienteLoggedIn');

            _reqSelectClienteLog.addEventListener('success', (ev2) => {
                var _emailCliente = ev2.target.result; //<---- email del cliente logueado...recupero sus datos....

                if (_emailCliente == undefined) {
                    refServicioNET.invokeMethodAsync('CallbackServIndexedDBblazor', null);
                    return;
                }

                var _reqSelectDatosCliente = _transac.objectStore('datosclientes').get(_emailCliente);
                _reqSelectDatosCliente.addEventListener('success', (evt) => {
                    var _datosCliente = evt.target.result;
                    //paso datos del cliente recuperados desde indexedDB al servicio blazor usando la ref.
                    refServicioNET.invokeMethodAsync('CallbackServIndexedDBblazor', _datosCliente);

                });
                _reqSelectDatosCliente.addEventListener('error', (err) => console.log(`error al recuperar datoscliente con emnail: ${email}...`, err));
            }); //cierre success-_reqSelectClienteLog
            _reqSelectClienteLog.addEventListener('error', (er2) => console.log('no hay datos de cliente logueado...'));

        });
    },
    almacenarToken: (token) => {

        var _reqDB = indexedDB.open('agapea2024', 1);

        _reqDB.addEventListener('upgradeneeded', (ev) => _crearEsquemaIndexedDB(ev));
        _reqDB.addEventListener('error', (error) => console.log('error al abrir bd agepa2024 de indexedDB....', error));
        _reqDB.addEventListener('success', (ev) => {
                                        var _db = ev.target.result;
                                        var _transac = _db.transaction(['tokens','clienteLogueado'],'readwrite');

                                        var _reqSelectClienteLog = _transac.objectStore('clienteLogueado').get('clienteLoggedIn');

                                        _reqSelectClienteLog.addEventListener('success', (ev2) => {
                                            var _emailCliente = ev2.target.result; //<---- email del cliente logueado...

                                            if (_emailCliente == undefined) return;
                                            var _insertJWT = _transac.objectStore('tokens').add(token, _emailCliente);
                                            _insertJWT.addEventListener('success', (evt) => console.log('token jwt insertado ok..', token));
                                            _insertJWT.addEventListener('error', (error) => { console.log('error insert token JWT', error); _transac.abort(); });
            }
        );

            _reqSelectClienteLog.addEventListener('error', (er2) => console.log('no hay datos de cliente logueado...'));
        });


    },
    recuperarTokenCliente: (refServicioNET,email) => {
        var _reqDB = indexedDB.open('agapea2024', 1);

        _reqDB.addEventListener('error', (error) => console.log('error al abrir bd agepa2024 de indexedDB....', error));
        _reqDB.addEventListener('upgradeneeded', (ev) => _crearEsquemaIndexedDB(ev));
        _reqDB.addEventListener('success', (ev) => {
            //para recuperar datos asociados a un cliente, del objectStore 'datoscliente'
            //me creo objeto IDBTransaction de tipo readonly
            var _db = ev.target.result;

            var _transac = _db.transaction(['tokens', 'clienteLogueado'], 'readonly');

            var _reqSelectClienteLog = _transac.objectStore('clienteLogueado').get('clienteLoggedIn');

            _reqSelectClienteLog.addEventListener('success', (ev2) => {
                var _emailCliente = ev2.target.result; //<---- email del cliente logueado...recupero sus datos....

                if (_emailCliente == undefined) return;

                var _reqSelectDatosCliente = _transac.objectStore('tokens').get(_emailCliente);
                _reqSelectDatosCliente.addEventListener('success', (evt) => {
                    var _jwt = evt.target.result;
                    //paso datos del cliente recuperados desde indexedDB al servicio blazor usando la ref.
                    refServicioNET.invokeMethodAsync('CallbackServIndexedDBblazorJWT', _jwt);

                });
                _reqSelectDatosCliente.addEventListener('error', (err) => console.log(`error al recuperar datoscliente con emnail: ${email}...`, err));
            }); //cierre success-_reqSelectClienteLog
            _reqSelectClienteLog.addEventListener('error', (er2) => console.log('no hay datos de cliente logueado...'));

        });
    }
}