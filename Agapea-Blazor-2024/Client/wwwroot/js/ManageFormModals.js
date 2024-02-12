window.adminFormModal = {
    refModalAbierto: null, //<---- en esta prop.del objeto almaceno una ref. al modal abierto en este momento
    ShowModal: function (id) {
        this.refModalAbierto = window.bootstrap.Modal.getOrCreateInstance(document.getElementById(id))//new bootstrap.Modal(document.getElementById(id));
        this.refModalAbierto.show();
    },
    HideModal: function (id) {
        if (this.refModalAbierto == null || this.refModalAbierto == undefined) this.refModalAbierto = window.bootstrap.Modal.getOrCreateInstance(document.getElementById(id));
        this.refModalAbierto.hide();
        this.refModalAbierto = null;
    }
}