using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGridChemicals : MonoBehaviour {

	static int N = 48;
	static float dt = 0.1f;
	static float diff = 0.0f;
	static float visc = 0.0f;
	static float force = 5.0f;
	static float source = 100.0f;
	static bool dvel = false;

	static float[] u;
	static float[] v;
	static float[] u_prev;
	static float[] v_prev;
	static float[] dens;
	static float[] dens_prev;

	static float omx, omy, mx, my;


	static void free_data(){
		u = null;
		v = null;
		u_prev = null;
		v_prev = null;
		dens = null;
		dens_prev = null;
	}

	static void clear_data (){
		int size = (N + 2) * (N + 2);

		for (int i = 0 ; i < size; i++) {
			u[i] = 0.0f;
			v[i] = 0.0f;
			u_prev[i] = 0.0f;
			v_prev[i] = 0.0f;
			dens[i] = 0.0f;
			dens_prev[i] = 0.0f;
		}
	}

	static int allocate_data (){
		int size = (N + 2) * (N + 2);

		u = new float[size];
		v = new float[size];
		u_prev = new float[size];
		v_prev = new float[size];
		dens = new float[size];	
		dens_prev = new float[size];

		return ( 1 );
	}

	static void draw_velocity (){

		float h = 1.0f / N;

		Color c = new Color(1.0f, 1.0f, 1.0f, 1.0f);

		for (int i = 1; i <= N; i++) {
			float x = (i - 0.5f) * h;
			for (int j = 1; j <= N; j++) {
				float y = (j - 0.5f) * h;
		
				Debug.DrawLine(new Vector2(i, j), new Vector2(i + u[IX(i, j)], j + v[IX(i, j)]), Color.white);
			}
		}
	}

	static void draw_density (){

		float h = 1.0f / N;

		for (int i = 0; i < N; i++) {
			float x = (i - 0.5f) * h;
			for (int j = 0; j < N; j++) {
				float y = (j - 0.5f) * h;

				float d00 = dens[IX(i, j)];
				float d01 = dens[IX(i, j + 1)];
				float d10 = dens[IX(i + 1, j)];
				float d11 = dens[IX(i + 1, j + 1)];

				Int2 _tileGridPos = new Int2(i, j);
				GameGrid.GetInstance().SetLighting(_tileGridPos, GameGridMesh.VERTEX_INDEX_BOTTOM_LEFT, 	new Color(d00, d00, d00, 1.0f), _setAverage: true);
				GameGrid.GetInstance().SetLighting(_tileGridPos, GameGridMesh.VERTEX_INDEX_BOTTOM_RIGHT, 	new Color(d10, d10, d10, 1.0f), _setAverage: true);
				GameGrid.GetInstance().SetLighting(_tileGridPos, GameGridMesh.VERTEX_INDEX_TOP_LEFT, 		new Color(d11, d11, d11, 1.0f), _setAverage: true);
				GameGrid.GetInstance().SetLighting(_tileGridPos, GameGridMesh.VERTEX_INDEX_TOP_RIGHT, 		new Color(d01, d01, d01, 1.0f), _setAverage: true);
			}
		}
	}

	static void get_from_UI (ref float[] d, ref float[] u, ref float[] v){
		int size = (N+2)*(N+2);

		for (int i = 0; i < size; i++) {
			u[i] = v[i] = d[i] = 0.0f;
		}

		if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1)) return;
		
		int mouseX = (int)((Input.mousePosition.x / (float)Screen.width) * N);
		int mouseY = (int)(((Screen.height - Input.mousePosition.y) / (float)Screen.height) * N);

		if (mouseX < 0 || mouseX >= N || mouseY < 0 || mouseY >= N) return;

		if (Input.GetMouseButton(0)) {
			u[IX(mouseX,mouseY)] = force * (mx-omx);
			v[IX(mouseX,mouseY)] = force * (omy-my);
		}

		if (Input.GetMouseButton(1)) {
			d[IX(mouseX,mouseY)] = source;
		}

		omx = mx;
		omy = my;

		return;
	}

	static void key_func (){
		if (Input.GetKeyDown(KeyCode.C)){
			clear_data ();
		}
		if (Input.GetKeyDown(KeyCode.Q)){
			free_data ();
			Application.Quit();
		}
		if (Input.GetKeyDown(KeyCode.V)){
			dvel = !dvel;
		}
	}

	static void mouse_func (){
		omx = mx = Input.mousePosition.x;
		omx = my = Input.mousePosition.y;
	}

	static void motion_func( int x, int y ){
		mx = x;
		my = y;
	}

	static void idle_func(){
		get_from_UI ( ref dens_prev, ref u_prev, ref v_prev );
		vel_step ( N, ref u, ref v, ref u_prev, ref v_prev, visc, dt );
		dens_step ( N, ref dens, ref dens_prev, ref u, ref v, diff, dt );
	}

	static void display_func(){
		if (dvel) draw_velocity();
		else draw_density();
	}

	void Start(){
		allocate_data();
	}

	void Update (){
		key_func();
		mouse_func();
		idle_func();
		display_func();
	}

///////////////////////////////////////////////////////////////////

	static void add_source(int N, ref float[] x, ref float[] s, float dt){
		int size = (N + 2) * (N + 2);
		for (int i = 0; i < size; i++) { 
			x[i] += dt * s[i];
		}
	}

	static void set_bnd(int N, int b, ref float[] x){
		for (int i = 1; i <= N; i++){
			x[IX(0, i)] 	= b == 1 ? -x[IX(1, i)] : x[IX(1, i)];
			x[IX(N + 1, i)] = b == 1 ? -x[IX(N, i)] : x[IX(N, i)];
			x[IX(i, 0)] 	= b == 2 ? -x[IX(i, 1)] : x[IX(i, 1)];
			x[IX(i, N + 1)] = b == 2 ? -x[IX(i, N)] : x[IX(i, N)];
		}
		x[IX(0, 0)] 		= 0.5f * (x[IX(1, 0)] 		+ x[IX(0, 1)]);
		x[IX(0, N + 1)] 	= 0.5f * (x[IX(1, N + 1)] 	+ x[IX(0, N)]);
		x[IX(N + 1, 0)] 	= 0.5f * (x[IX(N, 0)] 		+ x[IX(N + 1, 1)]);
		x[IX(N + 1, N + 1)] = 0.5f * (x[IX(N, N + 1)] 	+ x[IX(N + 1, N)]);
	}

	static int IX(int i, int j) {
		return (i) + (N + 2) * (j);
	}

	static void SWAP(ref float[] x0, ref float[] x) {
		float[] tmp = x0;
		x0 = x;
		x = tmp;
	}

	static void lin_solve(int N, int b, ref float[] x, ref float[] x0, float a, float c){
		int i, j, k;

		for (k = 0; k < 20; k++){
			for (i = 1; i <= N; i++){
				for (j = 1; j <= N; j++){
					x[IX(i, j)] = (x0[IX(i, j)] + a * (x[IX(i - 1, j)] + x[IX(i + 1, j)] + x[IX(i, j - 1)] + x[IX(i, j + 1)])) / c;
				}
			}
	
			set_bnd(N, b, ref x);
		}
	}

	static void diffuse(int N, int b, ref float[] x, ref float[] x0, float diff, float dt){
		float a = dt * diff * N * N;
		lin_solve(N, b, ref x, ref x0, a, 1 + 4 * a);
	}

	static void advect(int N, int b, ref float[] d, ref float[] d0, ref float[] u, ref float[] v, float dt){
		int i, j, i0, j0, i1, j1;
		float x, y, s0, t0, s1, t1, dt0;

		dt0 = dt * N;
		for (i = 1; i <= N; i++){
			for (j = 1; j <= N; j++){
				x = i - dt0 * u[IX(i, j)]; y = j - dt0 * v[IX(i, j)];

				if (x < 0.5f) x = 0.5f; 
				if (x > N + 0.5f) x = N + 0.5f;

				if (y < 0.5f) y = 0.5f; 
				if (y > N + 0.5f) y = N + 0.5f; 

				i0 = (int)x;
				j0 = (int)y; 

				i1 = i0 + 1;
				j1 = j0 + 1;

				s1 = x - i0; s0 = 1 - s1; t1 = y - j0; t0 = 1 - t1;
				d[IX(i, j)] = s0 * (t0 * d0[IX(i0, j0)] + t1 * d0[IX(i0, j1)]) + s1 * (t0 * d0[IX(i1, j0)] + t1 * d0[IX(i1, j1)]);

			}
		}

		set_bnd(N, b, ref d);
	}

	static void project(int N, ref float[] u, ref float[] v, ref float[] p, ref float[] div){
		for (int i = 1; i <= N; i++){
			for (int j = 1; j <= N; j++){
				div[IX(i, j)] = -0.5f * (u[IX(i + 1, j)] - u[IX(i - 1, j)] + v[IX(i, j + 1)] - v[IX(i, j - 1)]) / N;
				p[IX(i, j)] = 0;
			}
		}

		set_bnd(N, 0, ref div); 
		set_bnd(N, 0, ref p);

		lin_solve(N, 0, ref p, ref div, 1, 4);

		for (int i = 1; i <= N; i++){
			for (int j = 1; j <= N; j++){
				u[IX(i, j)] -= 0.5f * N * (p[IX(i + 1, j)] - p[IX(i - 1, j)]);
				v[IX(i, j)] -= 0.5f * N * (p[IX(i, j + 1)] - p[IX(i, j - 1)]);
			}
		}
		set_bnd(N, 1, ref u); set_bnd(N, 2, ref v);
	}

	static void dens_step(int N, ref float[] x, ref float[] x0, ref float[] u, ref float[] v, float diff, float dt){
		add_source(N, ref x, ref x0, dt);
		SWAP(ref x0, ref x); diffuse(N, 0, ref x, ref x0, diff, dt);
		SWAP(ref x0, ref x); advect(N, 0, ref x, ref x0, ref u, ref v, dt);
	}

	static void vel_step(int N, ref float[] u, ref float[] v, ref float[] u0, ref float[] v0, float visc, float dt){
		add_source(N, ref u, ref u0, dt);
		add_source(N, ref v, ref v0, dt);
		SWAP(ref u0, ref u);
		diffuse(N, 1, ref u, ref u0, visc, dt);
		SWAP(ref v0, ref v);
		diffuse(N, 2, ref v, ref v0, visc, dt);
		project(N, ref u, ref v, ref u0, ref v0);
		SWAP(ref u0, ref u);
		SWAP(ref v0, ref v);
		advect(N, 1, ref u, ref u0, ref u0, ref v0, dt);
		advect(N, 2, ref v, ref v0, ref u0, ref v0, dt);
		project(N, ref u, ref v, ref u0, ref v0);
	}
}