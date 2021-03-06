﻿using MrVeggie.Models;
using MrVeggie.Models.Auxiliary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MrVeggie.Contexts {

    public class Sugestao {

        private readonly ReceitaContext _context_r;
        private readonly UtilizadorContext _context_u;
        private readonly IngredientesPassoContext _context_ip;
        private readonly AgendaContext _context_a;



        /// <summary>
        /// Construtor da classe Sugestao
        /// </summary>
        /// <param name="context_r">Contexto das receitas</param>
        /// <param name="context_u">Contexto dos utilizadores</param>
        /// <param name="context_ip">Contexto dos ingredientes passo</param>
        /// <param name="context_a">Contexto da agenda</param>
        public Sugestao(ReceitaContext context_r, UtilizadorContext context_u, IngredientesPassoContext context_ip, AgendaContext context_a) {
            _context_r = context_r;
            _context_u = context_u;
            _context_ip = context_ip;
            _context_a = context_a;
        }



        /// <summary>
        /// Método que retorna todas as receitas do sistema.
        /// </summary>
        /// <returns>Lista das receitas</returns>
        public List<Receita> getReceitas() {
            return _context_r.Receita.ToList();
        }



        /// <summary>
        /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// </summary>
        /// <returns></returns>
        public List<Receita> getSugestoes() {
            List<Receita> r = new List<Receita>();

            Random rand = new Random();
            r.Add(_context_r.Receita.Find(rand.Next(1,_context_r.Receita.Count())));
            return r;
        }



        /// <summary>
        /// Método que retorna a lista das receitas sugeridas para um utilizador
        /// </summary>
        /// <param name="email">Email do utilizador</param>
        /// <returns>Lista das receitas sugeridas</returns>
        public List<Receita> getSugestoes(string email) {
            Utilizador utilizador = _context_u.Utilizador.Where(u => u.email == email).First();
            List<Receita> r = new List<Receita>(4);

            if (utilizador != null) {
                int id_utilizador = utilizador.id_utilizador;
                r.Add(getReceitaSugeridaPorHistorico(id_utilizador));
                r.Add(getReceitaSugeridaPorReceitasFav(id_utilizador));
                r.Add(getReceitaSugeridaPorIngredientesFav(id_utilizador));
            }


            return r;
        }



        /// <summary>
        /// Método que retorna uma receita como sugestão a um utilizador, baseada nas suas receitas favoritas.
        /// </summary>
        /// <param name="id_utilizador">ID do utilizador</param>
        /// <returns>Receita sugerida</returns>
        private Receita getReceitaSugeridaPorReceitasFav(int id_utilizador) {
            Receita x = null;

            List<int> rIds = _context_u.UtilizadorReceitasPref.Where(u => u.utilizador_id == id_utilizador).Select(u => u.receita_id).ToList();

            if (rIds.Count() == 0) {
                Random random = new Random();
                while (x == null) {
                    int rInt = random.Next(1, _context_r.Receita.ToArray().Count());

                    x = _context_r.Receita.Find(rInt);
                }
            }
            else {
                List<Receita> receitas = _context_r.Receita.Where(r => rIds.Contains(r.id_receita)).ToList();

                Random random = new Random();
                int rInt = random.Next(0, receitas.Count()-1);

                x = receitas.ElementAt(rInt);
            }

            return x;
        }



        /// <summary>
        /// Método que retorna uma receita como sugestão a um utilizador, baseada nos seus ingredientes favoritos.
        /// </summary>
        /// <param name="id_utilizador">ID do utilizador</param>
        /// <returns>Receita sugerida</returns>
        private Receita getReceitaSugeridaPorIngredientesFav(int id_utilizador) {
            List<int> ingFav = _context_u.UtilizadorIngredientesPref
                                                            .Where(uip => uip.utilizador_id == id_utilizador)
                                                            .Select(uip => uip.ingrediente_id).ToList()
                                                            .ToList();

            List<Receita> receitas = _context_r.Receita.ToList();
            List<(int, Receita)> sugestoes = new List<(int, Receita)>();

            foreach (Receita r in receitas) {
                List<Passo> passos = _context_r.Passo.Where(p => p.receita_id == r.id_receita).ToList();
                List<int> ings = new List<int>();

                foreach (Passo p in passos) {
                    ings.AddRange(_context_ip.IngredientesPasso.Where(ip => ip.passo_id == p.id_passo).Select(i => i.ingrediente_id).ToList());
                }

                sugestoes.Add((ings.Intersect(ingFav).Count(), r));
            }

            sugestoes.Sort((x1, x2) => x1.Item1.CompareTo(x2.Item1));

            Random random = new Random();
            int rInt = random.Next(0, Math.Min(4, sugestoes.Count()-1));

            return sugestoes.ElementAt(rInt).Item2;
        }



        /// <summary>
        /// Método que retorna uma receita como sugestão a um utilizador, baseada no seu histórico.
        /// </summary>
        /// <param name="id_utilizador">ID do utilizador</param>
        /// <returns>Receita sugerida</returns>
        private Receita getReceitaSugeridaPorHistorico(int id_utilizador) {
            string email = _context_u.Utilizador.Where(u => u.id_utilizador == id_utilizador).First().email;

            IEnumerable<Receita> historico = getHistorico(email).Take(4);
            if (historico.Count() == 0) return _context_r.Receita.Find(1);

            List<int> historicoIng = new List<int>();

            foreach (Receita r in historico) {
                List<Passo> passos = _context_r.Passo.Where(p => p.receita_id == r.id_receita).ToList();

                foreach (Passo p in passos) {
                    historicoIng.AddRange(_context_ip.IngredientesPasso.Where(ip => ip.passo_id == p.id_passo).Select(i => i.ingrediente_id).ToList());
                }

            }

            List<Receita> receitas = _context_r.Receita.ToList();
            List<(int, Receita)> sugestoes = new List<(int, Receita)>();

            foreach (Receita r in receitas) {
                if (!historico.Contains(r)) {
                    List<Passo> passos = _context_r.Passo.Where(p => p.receita_id == r.id_receita).ToList();
                    List<int> ings = new List<int>();

                    foreach (Passo p in passos) {
                        ings.AddRange(_context_ip.IngredientesPasso.Where(ip => ip.passo_id == p.id_passo).Select(i => i.ingrediente_id).ToList());
                    }

                    sugestoes.Add((ings.Intersect(historicoIng).Count(), r));
                }
            }

            sugestoes.Sort((x1, x2) => x1.Item1.CompareTo(x2.Item1));

            Random random = new Random();
            int rInt = random.Next(0, Math.Min(4, sugestoes.Count()-1));

            return sugestoes.ElementAt(rInt).Item2;
        }



        /// <summary>
        /// Método que retorna a lista das receitas já executadas por um utilizador
        /// </summary>
        /// <param name="email">Email do utilizador</param>
        /// <returns></returns>
        public List<Receita> getHistorico(string email) {
            int id_utilizador = _context_u.Utilizador.Where(u => u.email.Equals(email)).First().id_utilizador;
            List<Receita> receitas = new List<Receita>();

            List<HistoricoUtilizador> historico_ids = _context_u.HistoricoUtilizador.Where(hu => hu.utilizador_id == id_utilizador).ToList();

            foreach (HistoricoUtilizador hu in historico_ids) {
                receitas.Add(_context_r.Receita.Find(hu.receita_id));
            }

            return receitas;
        }



        /// <summary>
        /// Método que retorna a lista com as entradas da agenda (completa) de um utilizador.
        /// </summary>
        /// <param name="email">Email do utilizador</param>
        /// <returns>Lista da agenda</returns>
        public List<Agenda> getAgenda(string email) {
            int id_utilizador = _context_u.Utilizador.Where(u => u.email == email).First().id_utilizador;

            List<Agenda> agenda = _context_a.Agenda.Where(a => a.utilizador_id == id_utilizador).ToList();
            List<Agenda> agenda_completa = new List<Agenda>();

            for (int dia = 0; dia < 7; dia++) {
                agenda_completa.Add(new Agenda(dia, 'a', -1, -1));
                agenda_completa.Add(new Agenda(dia, 'j', -1, -1));
            }

            foreach (Agenda a in agenda) {
                a.receita = _context_r.Receita.Find(a.receita_id);
                int to_remove = a.dia * 2;
                if (a.refeicao.Equals('j')) to_remove++;
                agenda_completa.RemoveAt(to_remove);
                agenda_completa.Insert(to_remove, a);
            }

            return agenda_completa;
        }
    }
}
