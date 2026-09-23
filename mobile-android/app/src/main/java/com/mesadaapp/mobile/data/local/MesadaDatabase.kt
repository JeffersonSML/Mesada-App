package com.mesadaapp.mobile.data.local

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase

@Database(
    entities = [TarefaEntity::class, ExecucaoPendenteEntity::class],
    version = 1,
    exportSchema = false,
)
abstract class MesadaDatabase : RoomDatabase() {
    abstract fun tarefaDao(): TarefaDao
    abstract fun execucaoPendenteDao(): ExecucaoPendenteDao

    companion object {
        @Volatile
        private var instancia: MesadaDatabase? = null

        fun obterInstancia(context: Context): MesadaDatabase =
            instancia ?: synchronized(this) {
                instancia ?: Room.databaseBuilder(
                    context.applicationContext,
                    MesadaDatabase::class.java,
                    "mesada.db",
                ).build().also { instancia = it }
            }
    }
}
