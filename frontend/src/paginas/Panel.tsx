import { useAuth } from '../auth/useAuth'

export function Panel() {
  const { usuario, idEmpresa } = useAuth()

  return (
    <section>
      <h1>Panel</h1>
      <p className="sutil">Resumen de la empresa activa.</p>

      <div className="rejilla">
        <div className="tarjeta">
          <h3>Sesión</h3>
          <p>{usuario?.fullName ?? usuario?.userName}</p>
          <p className="sutil">Empresa #{idEmpresa}</p>
        </div>
        <div className="tarjeta">
          <h3>Roles</h3>
          {usuario && usuario.roles.length > 0 ? (
            <ul>
              {usuario.roles.map((r) => (
                <li key={r}>{r}</li>
              ))}
            </ul>
          ) : (
            <p className="sutil">Sin roles asignados.</p>
          )}
        </div>
      </div>
    </section>
  )
}
