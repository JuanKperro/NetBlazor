//fichero javascript que define un objeto que cuelga directamente de "window" (para hacerlo global a todos los comp.blazor)
//con props y metodos para manejar datos usando localStorage del navegador

window.adminLocalStorage = {
    almacenarValor: function (clave, valor) {
        localStorage.setItem(clave, JSON.stringify(valor));
    },
    recuperarValor: function (clave) {
        return JSON.parse(localStorage.getItem(clave));
    }
}