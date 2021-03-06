﻿using MrVeggie.Models;
using MrVeggie.Models.Auxiliary;
using MrVeggie.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MrVeggie.Contexts {


    public class Preparacao {

        private readonly ReceitaContext _context_r;
        private readonly PassoContext _context_passo;
        private readonly IngredientesPassoContext _context_ip;
        private readonly IngredienteContext _context_ing;



        /// <summary>
        /// Construtor da classe Preparacao.
        /// </summary>
        /// <param name="context_r">Contexto das receitas</param>
        /// <param name="context_p">Contexto dos passos</param>
        /// <param name="context_ip">Contexto dos ingredientes do passo</param>
        /// <param name="context_i">Contexto dos ingredientes</param>
        public Preparacao(ReceitaContext context_r, PassoContext context_p, IngredientesPassoContext context_ip, IngredienteContext context_i) {
            _context_passo = context_p;
            _context_ip = context_ip;
            _context_ing = context_i;
            _context_r = context_r;
        }



        /// <summary>
        /// Método que retorna toda a informação relativa a uma receita.
        /// </summary>
        /// <param name="id_receita">ID da receita</param>
        /// <returns>Receita completa</returns>
        public Receita getDetalhesReceita(int id_receita) {
            Receita receita = _context_r.Receita.Find(id_receita);

            var passos = _context_r.Passo.Where(p => p.receita_id == id_receita);

            foreach (Passo p in passos) {
                receita.passos.Add(p);
            }

            Dictionary<Ingrediente, Quantidade> ingredientes = new Dictionary<Ingrediente, Quantidade>();

            foreach (var p in receita.passos) {
                Dictionary<Ingrediente, Quantidade> ings_passo = getIngredientesPasso(p.id_passo);

                foreach (var i in ings_passo) {
                    if (!ingredientes.ContainsKey(i.Key)) {
                        Quantidade q = new Quantidade(0, i.Value.unidade);
                        ingredientes.Add(i.Key, q);
                    } 
                    ingredientes[i.Key].add(i.Value.quantidade);

                }
            }
            receita.ingredientes = ingredientes;

            var utensilios_ids = _context_r.UtensiliosReceita.Where(ur => ur.receita_id == id_receita);

            receita.utensilios = new List<Utensilio>();
            foreach (UtensiliosReceita ur in utensilios_ids) {
                receita.utensilios.Add(_context_r.Utensilio.Find(ur.utensilio_id));
            }

            return receita;
        }



        /// <summary>
        /// Método que retorna os ingredientes e as respetivas quantidades de um passo.
        /// </summary>
        /// <param name="id_passo">ID do passo</param>
        /// <returns>Ingredintes e as suas quantidades</returns>
        public Dictionary<Ingrediente, Quantidade> getIngredientesPasso(int id_passo) {
            Dictionary<Ingrediente, Quantidade> ingredientes = new Dictionary<Ingrediente, Quantidade>();

            List<IngredientesPasso> ips = _context_ip.IngredientesPasso.Where(ip => ip.passo_id == id_passo).ToList();

            foreach (var ip in ips) {
                Ingrediente i = _context_ing.Ingrediente.Find(ip.ingrediente_id);
                string unidade = _context_ip.Unidade.Find(ip.unidade_id).desc;
                Quantidade q = new Quantidade(ip.quantidade, unidade);
                ingredientes.Add(i, q);

            }

            return ingredientes;
        }
    }


}
