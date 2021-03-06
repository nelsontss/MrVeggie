﻿using Microsoft.EntityFrameworkCore;
using MrVeggie.Models.Auxiliary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MrVeggie.Models {


    public class Agenda {

        [Key]
        public int dia { get; set; }

        public char refeicao { get; set; }


        [Column("utilizador")]
        public int utilizador_id { get; set; }

        public Utilizador utilizador { get; set; }


        [Column("receita")]
        public int receita_id { get; set; }

        public Receita receita { get; set; }


        public Agenda(int dia, char refeicao, int utilizador_id, int receita_id) {
            this.dia = dia;
            this.refeicao = refeicao;
            this.utilizador_id = utilizador_id;
            this.receita_id = receita_id;
        }
    }


    public class AgendaContext : DbContext {

        public AgendaContext(DbContextOptions<AgendaContext> options) : base(options) {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            modelBuilder.Entity<Agenda>().HasKey(hu => new { hu.dia, hu.refeicao, hu.utilizador_id });
            modelBuilder.Entity<IngredientesPasso>().HasKey(ip => new { ip.passo_id, ip.ingrediente_id });
            modelBuilder.Entity<UtilizadorReceitasPref>().HasKey(urp => new { urp.utilizador_id, urp.receita_id });
            modelBuilder.Entity<UtensiliosReceita>().HasKey(ut => new { ut.receita_id, ut.utensilio_id});
            modelBuilder.Entity<UtilizadorIngredientesPref>().HasKey(uip => new { uip.utilizador_id, uip.ingrediente_id });
            modelBuilder.Entity<HistoricoUtilizador>().HasKey(hu => new { hu.utilizador_id, hu.receita_id, hu.data_conf });


            modelBuilder.Entity<Agenda>()
                        .HasOne<Utilizador>(hu => hu.utilizador)
                        .WithMany(u => u.agenda)
                        .HasForeignKey(hu => hu.utilizador_id)
                        .HasConstraintName("FKAgenda98980");

            modelBuilder.Entity<Agenda>()
                        .HasOne<Receita>(hu => hu.receita)
                        .WithMany(u => u.agenda)
                        .HasForeignKey(hu => hu.receita_id)
                        .HasConstraintName("FKAgenda50798");

        }

        public DbSet<Receita> Receita { get; set; }
        public DbSet<Utilizador> Utilizador { get; set; }
        public DbSet<Agenda> Agenda { get; set; }

    }
}
