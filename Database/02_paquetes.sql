CREATE OR REPLACE PACKAGE pkg_seguridad AS
  PROCEDURE validar_usuario(
    p_nombre_usuario IN VARCHAR2,
    p_password_hash IN VARCHAR2,
    p_id_usuario OUT NUMBER,
    p_nombre OUT VARCHAR2,
    p_estado OUT NUMBER,
    p_mensaje OUT VARCHAR2
  );
END pkg_seguridad;
/

COMMIT;

CREATE OR REPLACE PACKAGE BODY pkg_seguridad AS
  PROCEDURE validar_usuario(
    p_nombre_usuario IN VARCHAR2,
    p_password_hash IN VARCHAR2,
    p_id_usuario OUT NUMBER,
    p_nombre OUT VARCHAR2,
    p_estado OUT NUMBER,
    p_mensaje OUT VARCHAR2
  ) AS
  BEGIN
    p_id_usuario := NULL;
    p_nombre := NULL;

    SELECT id_usuario, nombre_usuario
      INTO p_id_usuario, p_nombre
      FROM usuarios
     WHERE UPPER(nombre_usuario) = UPPER(p_nombre_usuario)
       AND password_hash = p_password_hash
       AND activo = 'S';

    p_estado := 1;
    p_mensaje := 'Autenticación correcta';
  EXCEPTION
    WHEN NO_DATA_FOUND THEN
      p_estado := 0;
      p_mensaje := 'Credenciales inválidas';
  END validar_usuario;
END pkg_seguridad;
/

COMMIT;

CREATE OR REPLACE PACKAGE pkg_clientes AS
  PROCEDURE listar(p_resultado OUT SYS_REFCURSOR);
  PROCEDURE obtener(p_id IN NUMBER, p_resultado OUT SYS_REFCURSOR);
  PROCEDURE insertar(
    p_nombres IN VARCHAR2, p_apellidos IN VARCHAR2,
    p_correo IN VARCHAR2, p_telefono IN VARCHAR2,
    p_id OUT NUMBER
  );

PROCEDURE actualizar(
    p_id IN NUMBER, p_nombres IN VARCHAR2, p_apellidos IN VARCHAR2,
    p_correo IN VARCHAR2, p_telefono IN VARCHAR2, p_filas OUT NUMBER
  );

PROCEDURE eliminar(p_id IN NUMBER, p_filas OUT NUMBER);
END pkg_clientes;
/

COMMIT;


CREATE OR REPLACE PACKAGE BODY pkg_clientes AS
  PROCEDURE listar(p_resultado OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN p_resultado FOR
      SELECT id_cliente, nombres, apellidos, correo, telefono, fecha_registro
        FROM clientes_a ORDER BY id_cliente DESC;
  END;

  PROCEDURE obtener(p_id IN NUMBER, p_resultado OUT SYS_REFCURSOR) AS
  BEGIN
    OPEN p_resultado FOR
      SELECT id_cliente, nombres, apellidos, correo, telefono, fecha_registro
        FROM clientes_a WHERE id_cliente = p_id;
  END;

  PROCEDURE insertar(
    p_nombres IN VARCHAR2, p_apellidos IN VARCHAR2,
    p_correo IN VARCHAR2, p_telefono IN VARCHAR2, p_id OUT NUMBER
  ) AS
  BEGIN
    INSERT INTO clientes_a (nombres, apellidos, correo, telefono)
    VALUES(TRIM(p_nombres), TRIM(p_apellidos), LOWER(TRIM(p_correo)), TRIM(p_telefono))
    RETURNING id_cliente INTO p_id;
  END;

  PROCEDURE actualizar(
    p_id IN NUMBER, p_nombres IN VARCHAR2, p_apellidos IN VARCHAR2,
    p_correo IN VARCHAR2, p_telefono IN VARCHAR2, p_filas OUT NUMBER
  ) AS
  BEGIN
    UPDATE clientes_a
       SET nombres = TRIM(p_nombres),
           apellidos = TRIM(p_apellidos),
           correo = LOWER(TRIM(p_correo)),
           telefono = TRIM(p_telefono)
     WHERE id_cliente = p_id;
    p_filas := SQL%ROWCOUNT;
  END;

  PROCEDURE eliminar(p_id IN NUMBER, p_filas OUT NUMBER) AS
  BEGIN
    DELETE FROM clientes_a WHERE id_cliente = p_id;
    p_filas := SQL%ROWCOUNT;
  END;
END pkg_clientes;
/

COMMIT;