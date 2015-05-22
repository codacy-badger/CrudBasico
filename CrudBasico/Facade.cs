﻿using CrudBasico.ClassesNegocio;
using CrudBasico.DTO;
using CrudBasico.Facade;
using CrudBasico.InterfacesDAL;
using CrudBasico.InterfacesDAL.Implementation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace CrudBasico.Facade
{
    public class FacadeMensagem : IDisposable
    {
        IMensagemDAL mensagemDAL = null;
        IMensagemUsuarioDAL mensagemUsuarioDAL = null;
        IMensagemUsuarioViewDTODAL mensagemDtoDAL = null;
        private DbConnection cnn;

        public FacadeMensagem(DbConnection cnn)
        {
            this.cnn = cnn;
            //Pode ser criado sob demanda ou seja em cada método que for utilizar
            //Coloquei aqui para ficar mais simples
            mensagemDAL = new MensagemDAL(cnn);
            mensagemUsuarioDAL = new MensagemUsuarioDAL(cnn);
            mensagemDtoDAL = new MensagemUsuarioViewDTODAL(cnn);
        }

        public void SalvarMensagem(MensagemCriacaoDTO obj, string UsuarioCriacao)
        {            
            try
            {
                if (cnn.State == System.Data.ConnectionState.Closed)
                    cnn.Open();

                //garante que se alguma das operações falharem, será feito rollback das operações
                //Pode utilizar qualquer um dos dois tipos de transações

                //using (TransactionScope transaction = new TransactionScope())
                using (var transaction = this.cnn.BeginTransaction())
                {
                    Mensagem objMensagem = new Mensagem();
                    obj.Descricao = obj.Descricao;
                    
                    mensagemDAL.Inserir(objMensagem);

                    foreach (var item in obj.Destinatarios)
                    {
                        MensagemUsuario mu = new MensagemUsuario();
                        mu.Destinatario = item;
                        mu.UsuarioCriacao = UsuarioCriacao;
                        mu.DtCriacao = DateTime.Now;
                        //MensagemID será atribuído pelo DAL
                        mu.MensagemID = objMensagem.MensagemID;

                        mensagemUsuarioDAL.Inserir(mu);
                    }

                    //faz o commit
                    //transaction.Complete();
                    //ou
                    transaction.Commit();

                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //sempre fecha a conexão como  banco de dados
                cnn.Close();
            }
        }

        public void Dispose()
        {
            //Descarta os elementos da memória
            mensagemDAL = null;
            mensagemUsuarioDAL = null;
            mensagemDtoDAL = null;
            cnn.Close();
            cnn = null;
        }
    }
}






