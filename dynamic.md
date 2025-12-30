# 1. Main characteristics of the game objects (Properties)

Our game is a real-time 2D strategy game in which the player summons units that
advance along a lane, confront opposing units, and attack the enemy base. The
central objects of the game are therefore the units, the bases, and the spawning
mechanisms.

Each unit has several numerical characteristics that define its behavior: movement
speed, health points, attack damage, and the interval between two attacks. At this
stage of the project, the player’s units and the enemy units voluntarily share the
same statistics in order to guarantee a basic balance and to facilitate the
analysis of the system.

The initial values chosen are a speed of 2, 20 health points, 5 damage points per
attack, and an interval of 0.5 seconds. These parameters can be adjusted directly
from the Unity editor, which makes it possible to easily tune the balancing during
the testing phases.

Balancing is based on simple calculations: with these values, a unit deals 10
damage points per second, and a fight between two identical units lasts about 2
seconds. A base with 200 health points can only be destroyed after about twenty
seconds of continuous attack, which prevents a victory that is too fast without a
lasting advantage.

These values serve as a reference for the future addition of new types of units,
such as units that are more resistant but slower, or units that are faster but
more fragile, without introducing a single dominant strategy.

# 2. Location of the main objects (Locations)

The game map is organized around three parallel lanes connecting the player’s base,
located on the left, to the enemy base, located on the right. Units are generated
near their respective base and then automatically advance along the lane on which
they appear.

The choice of several lanes makes it possible to distribute the fights in space
and to avoid a single confrontation immediately deciding the outcome of the game.
Each lane evolves in a relatively independent way, which creates situations in
which the player can dominate one lane while being in difficulty on another.

The spawn points are placed far enough from the opposing bases to leave time for
the units to move before combat, making the action readable and allowing the
player to anticipate enemy waves. The length of the lanes prevents a rapid
destruction of the bases and favors a gradual progression of the front line.

The simplicity of the map emphasizes timing and unit management decisions rather
than exploration. This structure constitutes a balanced base that is easily
extensible for the future addition of mechanics or gameplay variations.

<img width="1416" height="712" alt="image" src="https://github.com/user-attachments/assets/5352b0b5-f9c6-4e43-bed8-e98ca3213d0f" />


# 3. Behaviors of objects and characters (Behaviors)

The units in the game follow simple rules: as long as they do not encounter any
opponent, they automatically advance along their lane toward the enemy base. When
they come into contact with an opposing unit, they stop and attack at regular
intervals until one of the two is destroyed. If they reach the enemy base without
opposition, they attack it directly.

In the early levels, the enemies (NPCs) simply generate units at a fixed rhythm,
creating constant but predictable pressure. In more advanced levels, their
behavior becomes more intelligent: the spawn rate and the choice of lanes are
adapted according to the situation, which makes the enemy pressure more difficult
to anticipate.

From these relatively simple rules, more complex behaviors emerge. The fights
create front lines that move according to the advantage gained by each side, and a
small success can turn into a lasting advantage if units survive and continue
their progression. With several lanes, the player must constantly arbitrate
between defense and attack, which prevents the appearance of a single strategy
that is valid in all situations.


# 4. Economic system (Economy)

The game uses an internal economy based on gold, which is used only for summoning
units. The player earns gold by eliminating enemy units and by winning battles.

Depending on the difficulty level, the player may be required to replay certain
previous battles in order to accumulate enough gold to summon more powerful units
during the following encounters. This system introduces a form of progression and
forces the player to plan their choices rather than summoning units without
constraint.

The economy thus makes it possible to regulate the pace of the game and to
strengthen the strategic dimension without adding complex mechanics.


# 5. Information, point of view, and perception of the game

The player receives mainly visual information about the state of the game. They
can observe the units on each lane, their progression, and the evolution of the
fights in real time. They also have access to essential management information,
such as the amount of available gold as well as the cost of units and defenses.

The information remains voluntarily partial: the player does not know in advance
the enemy’s exact actions but can anticipate the situation by observing the
pressure exerted on each lane. The 2D lateral point of view offers a global vision
of the battlefield and facilitates quick decisions.

# 6. Player control, choices, and strategies

The control method of the game is direct and real-time. The player mainly acts by
summoning units or defenses whenever they wish, depending on the situation on the
different lanes and on the amount of gold available. Each action has an immediate
and visible effect on the state of the game.

This control method forces the player to make constant choices, particularly
regarding the right moment to spend resources, the lane to reinforce, and the type
of unit to summon. A decision taken too early or too late can quickly unbalance a
battle.

The game does not rely on a single dominant strategy. The player can adopt
different approaches, such as gradually reinforcing several lanes or focusing on a
more powerful attack after accumulating gold. These choices depend on the context
and evolve according to the pressure exerted by the enemy, which makes each game
and each battle different.

# 7. Strategic choices and possible strategies

During the game, the player must constantly make strategic decisions, notably
regarding gold management, the moment to summon units, and the lane in which to
invest. These choices directly influence the balance of the fights and the
evolution of the front lines.

Several strategies are possible. The player can adopt a progressive approach by
regularly reinforcing all lanes in order to contain enemy pressure, or on the
contrary save gold to launch a more powerful attack on a specific lane. None of
these strategies is systematically better: their effectiveness depends on the
situation and on the behavior of the enemy.

There is also no completely useless strategy. A strategy that may seem weak in a
given context can become relevant in another situation, for example when the
enemy concentrates its forces elsewhere. This variety of choices prevents the
appearance of a single dominant strategy and pushes the player to adapt
constantly.
