-- Fecha uma lacuna da modelagem original: para papel_alvo = 'comum', o
-- convite precisa apontar para QUAL UsuarioComum (já cadastrado na Web,
-- conforme docs/especificacao.md #fluxo-de-convite) está sendo vinculado ao
-- dispositivo — sem isso não é possível emitir o JWT do filho ao resgatar o
-- código. Para papel_alvo = 'master', o convite não referencia ninguém
-- (a pessoa convidada ainda vai criar a própria conta com e-mail/senha).
BEGIN;

ALTER TABLE convites_acesso
    ADD COLUMN usuario_comum_id uuid REFERENCES usuarios_comuns(id) ON DELETE CASCADE;

ALTER TABLE convites_acesso
    ADD CONSTRAINT ck_convites_acesso_alvo_coerente CHECK (
        (papel_alvo = 'comum' AND usuario_comum_id IS NOT NULL) OR
        (papel_alvo = 'master' AND usuario_comum_id IS NULL)
    );

CREATE INDEX ix_convites_acesso_usuario_comum_id ON convites_acesso (usuario_comum_id);

COMMIT;
