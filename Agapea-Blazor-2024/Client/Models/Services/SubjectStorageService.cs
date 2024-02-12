using AgapeaBlazor2024.Client.Models.Services.Interfaces;
using AgapeaBlazor2024.Shared;
using System.Reactive.Subjects;

namespace AgapeaBlazor2024.Client.Models.Services
{
    public class SubjectStorageService : IStorageService
    {
        /*
            servicio:
                behavioursubject<Cliente>
                ------------------------------
                    datos_cliente
                ------------------------------
                    ||                      ||
                  datos_cliente             datos
                    output                 input
                    |                       |
                RecuperarDatosCliente()     AlmacenarDatosCliente(datos)


                behavioursubject<String>
                --------------------------------
                    jwt
                --------------------------------
         
                behavioursubject<List<ItemPedido>>
                --------------------------------
                    datos_elementos_pedido
                --------------------------------
                    ||                         || 
                     | <--- variable privada    | <---- modifico variable privada, actualiza datos del observable
                RecuperarElementosPedido()    OperarElementosPedido(elemento,cantidad,operacion)
         */
        #region ...propiedades clase servicio .....
        public event EventHandler<Cliente> ClienteRecupIndexedDBEvent;

        private BehaviorSubject<Cliente> _clienteSubject=new BehaviorSubject<Cliente>(null);
        private BehaviorSubject<String> _jwtSubject=new BehaviorSubject<string>("");
        private BehaviorSubject<List<ItemPedido>> _itemsPedidoSubject = new BehaviorSubject<List<ItemPedido>>(new List<ItemPedido>());

        private Cliente _datosCliente = new Cliente(); //<----variable para almacenar datos del subject Cliente
        private String _datosJWT = ""; //<--------------------variable para almacenar datos del subject String
        private List<ItemPedido> _datosItemsPedido= new List<ItemPedido>(); //<---- variable para almacenar datos del subject List<ItemPedido>

        #endregion

        public SubjectStorageService()
        {
                IDisposable _subscripItemsPedidoSubject = this._itemsPedidoSubject
                                                                .Subscribe<List<ItemPedido>>(
                                                                (List<ItemPedido> items)=> this._datosItemsPedido=items
                                                            );

                IDisposable _subscripClienteSubject=this._clienteSubject
                                                        .Subscribe<Cliente>(
                                                            (Cliente datosObs)=> this._datosCliente=datosObs
                                                            );

                IDisposable _jwtSubject = this._jwtSubject
                                            .Subscribe<String>(
                                                (String datosJWT) => this._datosJWT = datosJWT
                                            );
        }

        #region ...metodos sincronos....

        public List<ItemPedido> RecuperarItemsPedido()
        {
            return this._datosItemsPedido;
        }

        public void OperarItemsPedido(Libro libro, int cantidad, string operacion)
        {
            //en funcion del valor del parametro "operacion" actualizo datos del observable del subject....
            //añadir <--- añadir nuevo ItemPedido a la lista de items, comprobando antes si no existe (si existe inc.la cantidad)
            //borrar <----borrar ItemPedido de la lista de items
            //modificar  <---- modificar cantidad de ItemPedido

            int _posItem = this._datosItemsPedido.FindIndex((ItemPedido item) => item.LibroItem.ISBN13 == libro.ISBN13);

            switch (operacion)
            {
                case "añadir":
                    if (_posItem != -1)
                    {
                        //el libro existe, incremento cantidad...
                        this._datosItemsPedido[_posItem].CantidadItem += cantidad;
                    } else
                    {
                        //libro no existe en lista de items del pedido, añado nuevo itempedido 
                        this._datosItemsPedido.Add(new ItemPedido { LibroItem=libro, CantidadItem=1});
                    }
                    break;

                case "borrar":
                    if (_posItem != -1) this._datosItemsPedido.RemoveAt(_posItem);
                    break;

                case "modificar":
                    if (_posItem != -1) this._datosItemsPedido[_posItem].CantidadItem = cantidad;
                    break;

                default:
                    break;
            }


            //..actualizamos valor del observable del subject...
            this._itemsPedidoSubject.OnNext(this._datosItemsPedido);

        }

        public void AlmacenamientoDatosCliente(Cliente datoscliente)
        {
            this._clienteSubject.OnNext(datoscliente); //<---- actualizo datos en observable Cliente
            this.ClienteRecupIndexedDBEvent.Invoke(this,datoscliente); //<----disparo evento de actualizacion de datos del cliente por si alguien escucha el evento....
        }

        public void AlmacenamientoJWT(string jwt)
        {
            this._jwtSubject.OnNext(jwt); //<------ actualizo datos en observable String...
        }

        public Cliente RecuperarDatosCliente()
        {
            return this._datosCliente;
        }
        public string RecuperarJWT()
        {
            return this._datosJWT;
        }

        #endregion

        #region ...metodos asincronos (no implementados en este servicio)....
        public Task AlmacenamientoDatosClienteAsync(Cliente datoscliente)
        {
            throw new NotImplementedException();
        }


        public Task AlmacenamientoJWTAsync(string jwt)
        {
            throw new NotImplementedException();
        }


        public Task<Cliente> RecuperarDatosClienteAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> RecuperarJWTAsync()
        {
            throw new NotImplementedException();
        }



        public Task<List<ItemPedido>> RecuperarItemsPedidoAsync()
        {
            throw new NotImplementedException();
        }

        public Task OperarItemsPedidoAsync(Libro libro, int cantidad, string operacion)
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}
