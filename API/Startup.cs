using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.DTOs.Enuns;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelsViews;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;
using MinimalApi.DTOs;

namespace minimal_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private string key = "";
        public IConfiguration Configuration { get; set; }

        public void ConfigureService(IServiceCollection services)
        {

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization();

            services.AddScoped<iAdministradorServicos, AdministradorServico>();
            services.AddScoped<IVeiculosServicos, VeiculoServicos>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o toke Jwt"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                {
                    new OpenApiSecurityScheme{
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
                        });
            });


            services.AddDbContext<DbContexto>(options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString("ConexaoPadrao")
                );
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                #region Home
                endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
                #endregion

                #region Administradores

                string GerarTokenJwt(Administrador administrador)
                {
                    if (string.IsNullOrEmpty(key)) return string.Empty;
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim(ClaimTypes.Role, administrador.Perfil),
        new Claim("Perfil", administrador.Perfil)
    };
                    var token = new JwtSecurityToken(
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: credentials
                    );
                    return new JwtSecurityTokenHandler().WriteToken(token);

                }


                endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, iAdministradorServicos administradorServicos) =>
                {
                    var adm = administradorServicos.Login(loginDTO);
                    if (adm != null)
                    {
                        string token = GerarTokenJwt(adm);
                        return Results.Ok(new AdmLogado
                        {
                            Email = adm.Email,
                            Perfil = adm.Perfil,
                            Token = token
                        });
                    }
                    else
                    {
                        return Results.Unauthorized();
                    }
                }).AllowAnonymous().WithTags("Administrador");

                endpoints.MapGet("/administradores", ([FromQuery] int? pagina, iAdministradorServicos administradorServicos) =>
                {
                    var adms = new List<AdministradorModelView>();
                    var administradores = administradorServicos.Todos(pagina);
                    foreach (var adm in administradores)
                    {
                        adms.Add(new AdministradorModelView
                        {
                            Id = adm.Id,
                            Email = adm.Email,
                            Perfil = adm.Perfil
                        });
                    }
                    return Results.Ok(adms);

                }).RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Administrador");

                endpoints.MapGet("/administradores/{id}", ([FromRoute] int id, iAdministradorServicos administradorServicos) =>
                {
                    var administrador = administradorServicos.BuscaPorId(id);
                    if (administrador == null) return Results.NotFound();
                    return Results.Ok(new AdministradorModelView
                    {
                        Id = administrador.Id,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                    });

                }).RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Administrador");

                endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, iAdministradorServicos administradorServicos) =>
                {
                    var validacao = new ErroValidacao
                    {
                        Mensagem = new List<string>()
                    };

                    if (string.IsNullOrEmpty(administradorDTO.Email))
                    {
                        validacao.Mensagem.Add("Email não pode ser vazio");
                    }
                    if (string.IsNullOrEmpty(administradorDTO.Senha))
                    {
                        validacao.Mensagem.Add("A senha não deve ser vazia");
                    }
                    if (administradorDTO.Perfil == null)
                    {
                        validacao.Mensagem.Add("O perfil não pode estar em branco");
                    }
                    if (validacao.Mensagem.Count > 0)
                    {
                        return Results.BadRequest(validacao);
                    }

                    var administrador = new Administrador
                    {
                        Email = administradorDTO.Email,
                        Senha = administradorDTO.Senha,
                        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
                    };


                    administradorServicos.Incluir(administrador);
                    return Results.Created($"/administrador/{administrador}", new AdministradorModelView
                    {
                        Id = administrador.Id,
                        Email = administrador.Email,
                        Perfil = administrador.Perfil
                    });

                }).RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Administrador");
                #endregion

                #region Veiculos
                ErroValidacao validaDTO(VeiculoDTO veiculoDTO)
                {
                    var validacao = new ErroValidacao
                    {
                        Mensagem = new List<string>()
                    };

                    if (string.IsNullOrEmpty(veiculoDTO.Nome))
                    {
                        validacao.Mensagem.Add("O nome não pode ser vazio");
                    }
                    if (string.IsNullOrEmpty(veiculoDTO.Marca))
                    {
                        validacao.Mensagem.Add("A marca não deve estar em branco");
                    }
                    if (veiculoDTO.Ano < 1950)
                    {
                        validacao.Mensagem.Add("Veículo muito antigo, só é aceito anos superiores a 1949");
                    }
                    return validacao;
                }

                endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculosServicos veiculosServicos) =>
                {

                    var validacao = validaDTO(veiculoDTO);
                    if (validacao.Mensagem.Count > 0)
                    {
                        return Results.BadRequest(validacao);
                    }

                    var veiculo = new Veiculo
                    {
                        Nome = veiculoDTO.Nome,
                        Marca = veiculoDTO.Marca,
                        Ano = veiculoDTO.Ano
                    };
                    veiculosServicos.Incluir(veiculo);

                    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
                }).RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
                .WithTags("Veiculos");

                endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculosServicos veiculosServicos) =>
                {
                    var veiculos = veiculosServicos.Todos(pagina);

                    return Results.Ok(veiculos);
                }).RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
                .WithTags("Veiculos");

                endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculosServicos veiculosServicos) =>
                {
                    var veiculos = veiculosServicos.BuscaPorId(id);

                    if (veiculos == null) return Results.NotFound();

                    return Results.Ok(veiculos);
                }).RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
                .WithTags("Veiculos");

                endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculosServicos veiculosServicos) =>
                {
                    var veiculos = veiculosServicos.BuscaPorId(id);

                    if (veiculos == null) return Results.NotFound();

                    var validacao = validaDTO(veiculoDTO);
                    if (validacao.Mensagem.Count > 0)
                    {
                        return Results.BadRequest(validacao);
                    }

                    veiculos.Nome = veiculoDTO.Nome;
                    veiculos.Marca = veiculoDTO.Marca;
                    veiculos.Ano = veiculoDTO.Ano;

                    veiculosServicos.Atualizar(veiculos);

                    return Results.Ok(veiculos);
                }).RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Veiculos");

                endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculosServicos veiculosServicos) =>
                {
                    var veiculos = veiculosServicos.BuscaPorId(id);

                    if (veiculos == null) return Results.NotFound();

                    veiculosServicos.Apagar(veiculos);

                    return Results.NoContent();
                }).RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Veiculos");
                #endregion


            });
        }
    }
}