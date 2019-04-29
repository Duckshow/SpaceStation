using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class ElementSimulator : MonoBehaviour {

	unsafe struct Bin{ // WARNING: variables must correspond to ElementSimulator.compute's Bin!
		public uint ID;
		public uint PosX;
		public uint PosY;
		public uint IsDirty;

		public uint Load;
		public fixed uint Contents[BIN_MAX_AMOUNT_OF_CONTENT];
		public uint ClusterLoad;
		public fixed uint ClusterContents[BIN_CLUSTER_CONTENT_MAX];
		
		public Vector4 Color;

		public static int GetStride() {
			return sizeof(uint) * 6 + sizeof(uint) * BIN_MAX_AMOUNT_OF_CONTENT + sizeof(uint) * BIN_CLUSTER_CONTENT_MAX + sizeof(float) * 4; // must correspond to variables!
		}
	};

	unsafe struct Particle{ // WARNING: variables must correspond to ElementSimulator.compute's Particle!
		// private const int ELEMENT_COUNT = 2;
		// private const float PARTICLE_MASS = 1.0f;
		// private const float PARTICLE_VISC = 60.0f;
		// private const float H = 0.5f;
		// private const float HSQ = H * H;
		// public const float HSQ_TEMPERATURE = HSQ * 5.0f;
		// public const float MAX_TEMPERATURE = 1000.0f;
		// private const float THERMAL_DIFFUSIVITY = 0.25f;
		// private const float REPEL_STRENGTH_MIN = 2.0f;
		// private const float REPEL_STRENGTH_MAX = 2.0f;
		// private const float REPEL_FACTOR_MIN = 1.0f / REPEL_STRENGTH_MAX;
		// private const float REPEL_FACTOR_MAX = 1.0f / REPEL_STRENGTH_MIN;

		// private static readonly float[] MASS = new float[ELEMENT_COUNT] { 
		// 	PARTICLE_MASS * 1.0f,
		// 	PARTICLE_MASS * 1.0f,
		// };
		// private static readonly float[] VISCOSITY = new float[ELEMENT_COUNT] {
		// 	PARTICLE_VISC * 1.0f,
		// 	PARTICLE_VISC * 1.0f,
		// };
		// private static readonly float[] TEMPERATURE_FREEZING_POINT = new float[ELEMENT_COUNT] {
		// 	273.15f,
		// 	273.15f,
		// };
		// private static readonly float[] TEMPERATURE_BOILING_POINT = new float[ELEMENT_COUNT] {
		// 	373.15f,
		// 	373.15f,
		// };
		// private static readonly float[] REPEL_STRENGTH_SOLID = new float[ELEMENT_COUNT] {
		// 	1.75f,
		// 	1.75f,
		// };
		// private static readonly float[] REPEL_STRENGTH_LIQUID = new float[ELEMENT_COUNT] {
		// 	1.5f,
		// 	1.5f,
		// };
		// private static readonly float[] REPEL_STRENGTH_GAS = new float[ELEMENT_COUNT] {
		// 	2.0f,
		// 	2.0f,
		// };
		// private static readonly float[] THERMAL_DIFFUSIVITY_SOLID = new float[ELEMENT_COUNT] {
		// 	1.0f,
		// 	1.0f,
		// };
		// private static readonly float[] THERMAL_DIFFUSIVITY_LIQUID = new float[ELEMENT_COUNT] {
		// 	0.75f,
		// 	0.75f,
		// };
		// private static readonly float[] THERMAL_DIFFUSIVITY_GAS = new float[ELEMENT_COUNT] {
		// 	0.5f,
		// 	0.5f,
		// };
		// private static readonly float[] AMOUNT_TRANSFER_MOD_SOLID = new float[ELEMENT_COUNT] {
		// 	0.0f,
		// 	0.0f,
		// };
		// private static readonly float[] AMOUNT_TRANSFER_MOD_LIQUID = new float[ELEMENT_COUNT] {
		// 	0.15f,
		// 	0.15f,
		// };
		// private static readonly float[] AMOUNT_TRANSFER_MOD_GAS = new float[ELEMENT_COUNT] {
		// 	0.5f,
		// 	0.5f,
		// };
		// private static readonly Color32[] COLOR_SOLID = new Color32[ELEMENT_COUNT] {
		// 	new Color(0.75f, 1.0f, 1.0f, 1.0f),
		// 	new Color(1.0f, 1.0f, 0.25f, 1.0f),
		// };
		// private static readonly Color32[] COLOR_LIQUID = new Color32[ELEMENT_COUNT] {
		// 	new Color(0.5f, 1.0f, 1.0f, 0.75f),
		// 	new Color(1.0f, 1.0f, 0.0f, 0.75f),
		// };
		// private static readonly Color32[] COLOR_GAS = new Color32[ELEMENT_COUNT] {
		// 	new Color(1.0f, 1.0f, 1.0f, 0.5f),
		// 	new Color(1.0f, 1.0f, 0.5f, 0.5f),
		// };
		
		public Vector2 Pos;
		public Vector2 Velocity;
		public Vector2 Force;
		public float Amount;
		public float AmountStartFrame;
		public float Density;
		public float Pressure;
		public float Temperature;
		public float TemperatureStartFrame;
		public float RepelFactor;
		public float IsActive; // every thread needs a particle, so some will get inactive particles instead
		public Vector4 ParticlesToHeat;
		public Vector4 HeatToGive;
		public Vector4 ParticlesToGiveAmountTo;
		public Vector4 AmountToGive;

		public uint ElementIndex;
		public uint BinID;


		public static int GetStride() { // should preferably be multiple of 128
			return sizeof(float) * 31 + sizeof(uint) * 2; // must correspond to variables!
		}

		public void BlendVariablesWith(Particle _otherParticle) {
			float _half = 0.5f;
			Pos = 					Vector2.Lerp(Pos, 					_otherParticle.Pos, 					_half);
			Velocity = 				Vector2.Lerp(Velocity, 				_otherParticle.Velocity, 				_half);
			Force = 				Vector2.Lerp(Force, 				_otherParticle.Force, 					_half);
			Density = 				Mathf.Lerp(Density, 				_otherParticle.Density, 				_half);
			Pressure = 				Mathf.Lerp(Pressure, 				_otherParticle.Pressure, 				_half);
			Temperature = 			Mathf.Lerp(Temperature, 			_otherParticle.Temperature, 			_half);
			TemperatureStartFrame = Mathf.Lerp(TemperatureStartFrame, 	_otherParticle.TemperatureStartFrame, 	_half);
			RepelFactor = 			Mathf.Lerp(RepelFactor, 			_otherParticle.RepelFactor, 			_half);
			IsActive = 1.0f;
		}

		public void Clear() {
			Velocity = Vector2.zero;
			Force = Vector2.zero;
			Amount = 0.0f;
			Density = 0.0f;
			Pressure = 0.0f;
			Temperature = 0.0f;
			TemperatureStartFrame = 0.0f;
			RepelFactor = 0.0f;
			IsActive = 0.0f;
			ParticlesToHeat = Vector4.zero;
			HeatToGive = Vector4.zero;
			ParticlesToGiveAmountTo = Vector4.zero;
			AmountToGive = Vector4.zero;
			ElementIndex = 0;
			BinID = 0;
		}

		// public void SetTemperature(float _temp){
		// 	Temperature = _temp;

		// 	float repelStrengthSolid = GetRepelStrengthSolid();
		// 	float repelStrengthLiquid = GetRepelStrengthLiquid();
		// 	float repelStrengthGas = GetRepelStrengthGas();

		// 	const float STATE_SHIFT_SMOOTHING_START = 0.9f;
		// 	float freezingPoint = GetFreezingPoint();
		// 	float freezingPointSmoothStart = GetFreezingPoint() * STATE_SHIFT_SMOOTHING_START;
		// 	float boilingPoint = GetBoilingPoint(); 
		// 	float boilingPointSmoothStart = GetBoilingPoint() * STATE_SHIFT_SMOOTHING_START;

		// 	// to prevent melting causing explosions, lerp the repelstrength
		// 	float progressSolidToLiquid = Mathf.Clamp01((_temp - freezingPointSmoothStart) / (freezingPoint - freezingPointSmoothStart));
		// 	float progressLiquidToGas = Mathf.Clamp01((_temp - boilingPointSmoothStart) / (boilingPoint - boilingPointSmoothStart));
		// 	float repelStrengthSmoothedSolidToLiquid = Mathf.Lerp(repelStrengthSolid, repelStrengthLiquid, progressSolidToLiquid);
		// 	float repelStrengthSmoothedLiquidToGas = Mathf.Lerp(repelStrengthLiquid, repelStrengthGas, progressLiquidToGas);

		// 	// each state has a fixed strength, but gas continues the more temperature increases
		// 	RepelFactor = 0.0f;
		// 	if(IsSolid()) 	{ RepelFactor += 1.0f / repelStrengthSmoothedSolidToLiquid; }
		// 	if(IsLiquid()) 	{ RepelFactor += 1.0f / repelStrengthSmoothedLiquidToGas; }
		// 	if(IsGas()) 	{ RepelFactor += 1.0f / repelStrengthGas; }

		// 	if (IsGas()){
		// 		float _repelStrengthFromTemperature = Mathf.Max(Temperature / MAX_TEMPERATURE * REPEL_STRENGTH_MAX, REPEL_STRENGTH_MIN);
		// 		float extraRepelFactor = 1.0f / Mathf.Clamp(_repelStrengthFromTemperature, REPEL_STRENGTH_MIN, REPEL_STRENGTH_MAX);
		// 		RepelFactor += (extraRepelFactor - RepelFactor);
		// 	}

		// 	RepelFactor = Mathf.Clamp(RepelFactor, REPEL_FACTOR_MIN, REPEL_FACTOR_MAX); // just a safeguard
		// }

		// bool IsSolid() {
		// 	return TemperatureStartFrame < GetFreezingPoint();
		// }

		// bool IsLiquid() {
		// 	return !IsSolid() && !IsGas();
		// }

		// bool IsGas() {
		// 	return TemperatureStartFrame > GetBoilingPoint();
		// }

		// Color32 GetColor(){
		// 	if (IsSolid()) { 
		// 		return GetColorSolid(); 
		// 	}
		// 	else if (IsLiquid()) { 
		// 		return GetColorLiquid();
		// 	}
		// 	else { 
		// 		return GetColorGas();
		// 	}
		// }

		// public float GetThermalDiffusivity() {
		// 	if (IsSolid()) {
		// 		return GetThermalDiffusivitySolid() * THERMAL_DIFFUSIVITY;
		// 	}
		// 	else if (IsLiquid()) {
		// 		return GetThermalDiffusivityLiquid() * THERMAL_DIFFUSIVITY;
		// 	}
		// 	else {
		// 		return GetThermalDiffusivityGas() * THERMAL_DIFFUSIVITY;
		// 	}
		// }

		// float GetAmountTransferMod(){
		// 	if (IsSolid()) {
		// 		return GetAmountTransferModSolid() * AMOUNT_TRANSFER_MOD;
		// 	}
		// 	else if (IsLiquid()) {
		// 		return GetAmountTransferModLiquid() * AMOUNT_TRANSFER_MOD;
		// 	}
		// 	else {
		// 		return GetAmountTransferModGas() * AMOUNT_TRANSFER_MOD;
		// 	}
		// }

		// float GetRepelStrengthCurrent()	{ 
		// 	if (IsSolid()) {
		// 		return GetRepelStrengthSolid();
		// 	}
		// 	else if (IsLiquid()) {
		// 		return GetRepelStrengthLiquid();
		// 	}
		// 	else {
		// 		return GetRepelStrengthGas();
		// 	}
		// }

		// float GetMass() { return MASS[ElementIndex]; }
		// float GetViscosity() { return VISCOSITY[ElementIndex]; }
		// float GetFreezingPoint() { return TEMPERATURE_FREEZING_POINT[ElementIndex]; }
		// float GetBoilingPoint() { return TEMPERATURE_BOILING_POINT[ElementIndex]; }
		// float GetRepelStrengthSolid() { return REPEL_STRENGTH_SOLID[ElementIndex]; }
		// float GetRepelStrengthLiquid() { return REPEL_STRENGTH_LIQUID[ElementIndex]; }
		// float GetRepelStrengthGas() { return REPEL_STRENGTH_GAS[ElementIndex]; }
		// float GetThermalDiffusivitySolid() { return THERMAL_DIFFUSIVITY_SOLID[ElementIndex]; }
		// float GetThermalDiffusivityLiquid() { return THERMAL_DIFFUSIVITY_LIQUID[ElementIndex]; }
		// float GetThermalDiffusivityGas() { return THERMAL_DIFFUSIVITY_GAS[ElementIndex]; }
		// float GetAmountTransferModSolid() { return AMOUNT_TRANSFER_MOD_SOLID[ElementIndex]; }
		// float GetAmountTransferModLiquid() { return AMOUNT_TRANSFER_MOD_LIQUID[ElementIndex]; }
		// float GetAmountTransferModGas() { return AMOUNT_TRANSFER_MOD_GAS[ElementIndex]; }
		// Color32 GetColorSolid() { return COLOR_SOLID[ElementIndex]; }
		// Color32 GetColorLiquid() { return COLOR_LIQUID[ElementIndex]; }
		// Color32 GetColorGas() { return COLOR_GAS[ElementIndex]; }
	}

	private const int THREAD_COUNT_MAX = 1024;

	private const int START_PARTICLE_COUNT = 4096; // must be divisible by THREAD_COUNT_X!
	private static readonly int START_PARTICLE_COUNT_ACTIVE = GameGrid.SIZE.x * GameGrid.SIZE.y;

	private const float AMOUNT_TRANSFER_MOD = 0.25f;

	//#region[rgba(80, 0, 0, 1)] | WARNING: shared with ElementSimulator.compute! must be equal!

	private const int OUTPUT_THREAD_COUNT_X = 16;
	private const int OUTPUT_THREAD_COUNT_Y = 16;

	private const int BINS_THREAD_COUNT = 16;

	private const int THREAD_COUNT_X = 16;
	private const int PIXELS_PER_TILE_EDGE = 1;
	private const int GRID_WIDTH_TILES = 48;
	private const int GRID_HEIGHT_TILES = 48;
	private const int GRID_WIDTH_PIXELS = PIXELS_PER_TILE_EDGE * GRID_WIDTH_TILES;
	private const int GRID_HEIGHT_PIXELS = PIXELS_PER_TILE_EDGE * GRID_HEIGHT_TILES;
	private const int BIN_SIZE = 2;
	private const int BIN_COUNT_X = GRID_WIDTH_PIXELS / BIN_SIZE;
	private const int BIN_COUNT_Y = GRID_HEIGHT_PIXELS / BIN_SIZE;
	private const int BIN_MAX_AMOUNT_OF_CONTENT = 8;
	private const int BIN_CLUSTER_SIZE = 9;
	private const int BIN_CLUSTER_CONTENT_MAX = BIN_CLUSTER_SIZE * BIN_MAX_AMOUNT_OF_CONTENT;
	//#endregion

	// kernels
	private const string KERNEL_INIT = "Init";
	private const string KERNEL_INITBINS = "InitBins";
	private const string KERNEL_CLEAROUTPUTTEXTURE = "ClearOutputTexture";
	private const string KERNEL_CACHEPARTICLESINBINS = "CacheParticlesInBins";
	private const string KERNEL_CACHECLUSTERSINBINS = "CacheClustersInBins";
	private const string KERNEL_COMPUTEDENSITY = "ComputeDensity";
	private const string KERNEL_COMPUTEPRESSURE = "ComputePressure";
	private const string KERNEL_COMPUTEFORCES = "ComputeForces";
	private const string KERNEL_INTEGRATE = "Integrate";
	private int kernelID_Init;
	private int kernelID_InitBins;
	private int kernelID_ClearOutputTexture;
	private int kernelID_CacheParticlesInBins;
	private int kernelID_CacheClustersInBins;
	private int kernelID_ComputeDensity;
	private int kernelID_ComputePressure;
	private int kernelID_ComputeHeat;
	private int kernelID_ApplyHeat;
	private int kernelID_ComputeAmountTransfer;
	private int kernelID_ApplyAmountTransfer;
	private int kernelID_ComputeForces;
	private int kernelID_Integrate;

	// properties
	private const string PROPERTY_BINS = "bins";
	private const string PROPERTY_BINSATSTARTFRAME = "binsAtStartFrame";
	private const string PROPERTY_PARTICLES = "particles";
	private const string PROPERTY_PARTICLECOUNT = "particleCount";
	private const string PROPERTY_DEBUGVARS = "debugVars";
	private const string PROPERTY_OUTPUT = "output";
	private const string PROPERTY_ISFIRSTFRAME = "isFirstFrame";
	private const string PROPERTY_ISEVENFRAME = "isEvenFrame";
	private const string PROPERTY_DEBUGINDEX = "debugIndex";

	private int shaderPropertyID_bins;
	private int shaderPropertyID_binsAtStartFrame;
	private int shaderPropertyID_particles;
	private int shaderPropertyID_particleCount;
	private int shaderPropertyID_debugVars;
	private int shaderPropertyID_output;
	private int shaderPropertyID_isFirstFrame;
	private int shaderPropertyID_isEvenFrame;
	private int shaderPropertyID_debugIndex;

	private float updateInterval = 0.0f;//0.05f;
	private float nextTimeToUpdate = 0.0f;

	private float updateIntervalBins = 0.0f;// 0.075f;// 0.075f;// 0.075f;
	private float nextTimeToUpdateBins = 0.0f;

	private float updateIntervalHeat = 0.5f;
	private float nextTimeToUpdateHeat = 0.0f;

	private float updateIntervalAmountTransfer = 0.5f;
	private float nextTimeToUpdateAmountTransfer = 0.0f;

	private ComputeBuffer bufferBins;
	private ComputeBuffer bufferBinsAtStartFrame;
	private Bin[] bins;
	private Bin[] binsAtStartFrame;
	
	private ComputeBuffer bufferParticles;
	private Particle[] particles;

	private ComputeBuffer bufferDebug;

	private RenderTexture output;
	private Vector2[] uvs;

	[SerializeField]
	private ComputeShader shader;
	[SerializeField]
	private Material material;
	[SerializeField]
	private ParticleSystem particleSys;

	private ElementSimulator.Particle[,] particleGrid;
	private ElementSimulator.Particle[,] particleGridPrev;
	
	private struct ParticleToHeatData {
		public Int2 NodeGridPos;
		public float HeatToGive;
	}
	private List<ParticleToHeatData>[,] particlesToHeatGrid;

	private bool isFirstFrame = true;
	private int frame = 0;

	private Vector2 GetParticleGridOrigin() { 
		return GameGrid.GetInstance().transform.position + new Vector3(GameGrid.SIZE.x * -0.5f, GameGrid.SIZE.y * -0.5f, 0.0f);
	}


	void Awake(){
		kernelID_Init = shader.FindKernel(KERNEL_INIT);
		kernelID_InitBins = shader.FindKernel(KERNEL_INITBINS);
		kernelID_ClearOutputTexture = shader.FindKernel(KERNEL_CLEAROUTPUTTEXTURE);
		kernelID_CacheParticlesInBins = shader.FindKernel(KERNEL_CACHEPARTICLESINBINS);
		kernelID_CacheClustersInBins = shader.FindKernel(KERNEL_CACHECLUSTERSINBINS);
		kernelID_ComputeDensity = shader.FindKernel(KERNEL_COMPUTEDENSITY);
		kernelID_ComputePressure = shader.FindKernel(KERNEL_COMPUTEPRESSURE);
		kernelID_ComputeForces = shader.FindKernel(KERNEL_COMPUTEFORCES);
		kernelID_Integrate = shader.FindKernel(KERNEL_INTEGRATE);

		shaderPropertyID_bins = Shader.PropertyToID(PROPERTY_BINS);
		shaderPropertyID_binsAtStartFrame = Shader.PropertyToID(PROPERTY_BINSATSTARTFRAME);
		shaderPropertyID_particles = Shader.PropertyToID(PROPERTY_PARTICLES);
		shaderPropertyID_particleCount = Shader.PropertyToID(PROPERTY_PARTICLECOUNT);
		shaderPropertyID_output = Shader.PropertyToID(PROPERTY_OUTPUT);
		shaderPropertyID_isFirstFrame = Shader.PropertyToID(PROPERTY_ISFIRSTFRAME);
		shaderPropertyID_isEvenFrame = Shader.PropertyToID(PROPERTY_ISEVENFRAME);
		shaderPropertyID_debugIndex = Shader.PropertyToID(PROPERTY_DEBUGINDEX);
	}

	void OnDisable(){
		bufferBins.Dispose();
		bufferBinsAtStartFrame.Dispose();
		bufferParticles.Dispose();
	}
	
	void Start () {
		InitShader();
	}

	void InitShader(){
		transform.localScale = new Vector3(GRID_WIDTH_TILES, GRID_HEIGHT_TILES, 1);

		bins = new Bin[BIN_COUNT_X * BIN_COUNT_Y];
		binsAtStartFrame = new Bin[BIN_COUNT_X * BIN_COUNT_Y];

		output = new RenderTexture(GRID_WIDTH_PIXELS, GRID_HEIGHT_PIXELS, 24);
		output.enableRandomWrite = true;
		output.filterMode = FilterMode.Point;
		output.Create();

		particleGridPrev = new Particle[GameGrid.SIZE.x, GameGrid.SIZE.y];
		particleGrid = new Particle[GameGrid.SIZE.x, GameGrid.SIZE.y];
		particles = new Particle[START_PARTICLE_COUNT];

		particlesToHeatGrid = new List<ParticleToHeatData>[GameGrid.SIZE.x, GameGrid.SIZE.y];

		// for (int x = 0; x < GameGrid.SIZE.x; x++){
		// 	for (int y = 0; y < GameGrid.SIZE.y; y++){
		// 		Particle particle = particleGrid[x, y];

		// 		particle.Amount = x < 30 ? Random.value * 100.0f : 0;

		// 		if (x >= 22 && x <= 26){
		// 			if (y >= 22 && y <= 26){
		// 				particle.Velocity = (new Vector2(x, y) - new Vector2(24, 24)) * 100.0f;
		// 				// particle.Amount = 100;
		// 			}
		// 		}

		// 		particle.Pos = new Vector2(x + Random.value, y + Random.value);
		// 		particle.Temperature = Random.value * 1000;// y > 24 ? 1000 : 200;
		// 		particle.TemperatureStartFrame = particle.Temperature;
		// 		particle.IsActive = particle.Amount > 0 ? 1 : 0;

		// 		particleGrid[x, y] = particle;
		// 	}
		// }

		for (int i = 0; i < particles.Length; i++){
			Particle particle = particles[i];

			particle.Amount = 100;// i < 30 ? Random.value * 100.0f : 0;

			particle.Pos = new Vector2((i % GameGrid.SIZE.x) + Random.value, (i / (float)GameGrid.SIZE.x) + Random.value);
			particle.Temperature = Random.value * 1000;// y > 24 ? 1000 : 200;
			particle.TemperatureStartFrame = particle.Temperature;
			particle.IsActive = particle.Amount > 0 ? 1 : 0;

			particles[i] = particle;
		}

		// for (int i = 0; i < particles.Length; i++){
		// 	int x = i % GameGrid.SIZE.x;
		// 	int y = Mathf.FloorToInt(i / GameGrid.SIZE.x);

		// 	if (GameGrid.IsInsideNodeGrid(x, y)){
		// 		particles[i] = particleGrid[x, y];
		// 	}
		// 	else{
		// 		particles[i].Clear();
		// 	}
		// }

		bufferBins = new ComputeBuffer(bins.Length, Bin.GetStride());
		bufferBinsAtStartFrame = new ComputeBuffer(binsAtStartFrame.Length, Bin.GetStride());
		bufferParticles = new ComputeBuffer(particles.Length, Particle.GetStride());
	}

	void Update() {
		if (Time.time < nextTimeToUpdate) return;
		nextTimeToUpdate = Time.time + updateInterval;

		// int _particleIndex = 0;
		// for (int x = 0; x < GameGrid.SIZE.x; x++){
		// 	for (int y = 0; y < GameGrid.SIZE.y; y++){
		// 		_particleIndex = y * GameGrid.SIZE.x + x;
		// 		Particle _particle = particles[_particleIndex];

		// 		if (Mathf.Approximately(particleGrid[x, y].Amount, 0.0f)){
		// 			_particle.Clear();
		// 			_particle.Pos = GetParticleGridOrigin() + new Vector2(x, y);
		// 		}
		// 		else{
		// 			_particle = particleGrid[x, y];
		// 		}

		// 		particles[_particleIndex] = _particle;
		// 	}
		// }

		// while (_particleIndex < particles.Length - 1){
		// 	_particleIndex++;
		// 	Particle _particle = particles[_particleIndex];
		// 	_particle.Clear();
		// 	_particle.Pos = GetParticleGridOrigin();
		// 	particles[_particleIndex] = _particle;
		// }


		UpdateShader();

		for (int x = 0; x < GameGrid.SIZE.x; x++){
			for (int y = 0; y < GameGrid.SIZE.y; y++){
				GameGrid.GetInstance().SetLighting(new Int2(x, y), GameGridMesh.VERTEX_INDEX_BOTTOM_LEFT, Color.black);
				GameGrid.GetInstance().SetLighting(new Int2(x, y), GameGridMesh.VERTEX_INDEX_BOTTOM_RIGHT, Color.black);
				GameGrid.GetInstance().SetLighting(new Int2(x, y), GameGridMesh.VERTEX_INDEX_TOP_LEFT, Color.black);
				GameGrid.GetInstance().SetLighting(new Int2(x, y), GameGridMesh.VERTEX_INDEX_TOP_RIGHT, Color.black);
			}
		}

		// particleGridPrev = particleGrid;
		// particleGrid = new Particle[GameGrid.SIZE.x, GameGrid.SIZE.y];

		// for (int i = 0; i < particles.Length; i++){
		// 	Particle _particle = particles[i];
		// 	Int2 _nodeGridPos = new Int2(_particle.Pos.x, _particle.Pos.y);
		// 	if (!GameGrid.IsInsideNodeGrid(_nodeGridPos)){
		// 		continue;
		// 	}
		// 	if (Mathf.Approximately(_particle.Amount, 0.0f)){
		// 		continue;
		// 	}

		// 	if (_nodeGridPos.x < 20 && _nodeGridPos.y > 20 && _nodeGridPos.y < 30){
		// 		// SuperDebug.Mark(GameGrid.GetInstance().GetWorldPosFromNodeGridPos(_nodeGridPos), Color.white, _particle.Temperature);
		// 	}
		// 	AddParticleToParticleGrid(_nodeGridPos, _particle);
		// 	SuperDebug.MarkPoint(GetParticleGridOrigin() + _particle.Pos, new Color32((byte)Mathf.Lerp(0, 255, _particle.Temperature / 1000.0f), 0, 255, 255), 0.1f, Time.deltaTime);
		// }

		// float t = 0;
		// for (int x = 0; x < GameGrid.SIZE.x; x++){
		// 	for (int y = 0; y < GameGrid.SIZE.y; y++){
		// 		Particle _particle = particleGrid[x, y];

		// 		if (x > 0){
		// 			Int2 _nodeGridPos = new Int2(x - 1, y);
		// 			Particle _neighbor = particleGrid[_nodeGridPos.x, _nodeGridPos.y];
		// 			Particle _neighborPrev = particleGridPrev[_nodeGridPos.x, _nodeGridPos.y];

		// 			float _delta = (_neighborPrev.Amount - _particle.Amount) * AMOUNT_TRANSFER_MOD;
		// 			if (_delta > 0.0f){
		// 				_particle.Amount += _delta;
		// 				_neighbor.Amount -= _delta;
		// 			}

		// 			particleGrid[_nodeGridPos.x, _nodeGridPos.y] = _neighbor;
		// 		}

		// 		if (x < GameGrid.SIZE.x - 1){
		// 			Int2 _nodeGridPos = new Int2(x + 1, y);
		// 			Particle _neighbor = particleGrid[_nodeGridPos.x, _nodeGridPos.y];
		// 			Particle _neighborPrev = particleGridPrev[_nodeGridPos.x, _nodeGridPos.y];

		// 			float _delta = (_neighborPrev.Amount - _particle.Amount) * AMOUNT_TRANSFER_MOD;
		// 			if (_delta > 0.0f){
		// 				_particle.Amount += _delta;
		// 				_neighbor.Amount -= _delta;
		// 			}

		// 			particleGrid[_nodeGridPos.x, _nodeGridPos.y] = _neighbor;
		// 		}

		// 		if (y > 0){
		// 			Int2 _nodeGridPos = new Int2(x, y - 1);
		// 			Particle _neighbor = particleGrid[_nodeGridPos.x, _nodeGridPos.y];
		// 			Particle _neighborPrev = particleGridPrev[_nodeGridPos.x, _nodeGridPos.y];

		// 			float _delta = (_neighborPrev.Amount - _particle.Amount) * AMOUNT_TRANSFER_MOD;
		// 			if (_delta > 0.0f){
		// 				_particle.Amount += _delta;
		// 				_neighbor.Amount -= _delta;
		// 			}

		// 			particleGrid[_nodeGridPos.x, _nodeGridPos.y] = _neighbor;
		// 		}

		// 		if (y < GameGrid.SIZE.y - 1){
		// 			Int2 _nodeGridPos = new Int2(x, y + 1);
		// 			Particle _neighbor = particleGrid[_nodeGridPos.x, _nodeGridPos.y];
		// 			Particle _neighborPrev = particleGridPrev[_nodeGridPos.x, _nodeGridPos.y];

		// 			float _delta = (_neighborPrev.Amount - _particle.Amount) * AMOUNT_TRANSFER_MOD;
		// 			if (_delta > 0.0f){
		// 				_particle.Amount += _delta;
		// 				_neighbor.Amount -= _delta;
		// 			}

		// 			particleGrid[_nodeGridPos.x, _nodeGridPos.y] = _neighbor;
		// 		}
		// 		t += _particle.Amount;

		// 		particleGrid[x, y] = _particle;
		// 	}
		// }
		// Debug.Log(t);


		// for (int x = 0; x < GameGrid.SIZE.x; x++){
		// 	for (int y = 0; y < GameGrid.SIZE.y; y++){
		// 		Particle _particle = particleGrid[x, y];
		// 		if (_particle.IsActive == 0.0f){
		// 			return;
		// 		}

		// 		Int2 _nodeGridPos = new Int2(x, y);
		// 		PopulateParticleToHeatGridAtPos(_nodeGridPos);

		// 		TrySetHeatTransferToParticle(_nodeGridPos, _nodeGridPos + Int2.Up);
		// 		TrySetHeatTransferToParticle(_nodeGridPos, _nodeGridPos + Int2.Down);
		// 		TrySetHeatTransferToParticle(_nodeGridPos, _nodeGridPos + Int2.Left);
		// 		TrySetHeatTransferToParticle(_nodeGridPos, _nodeGridPos + Int2.Right);

		// 		foreach (ParticleToHeatData _pth in particlesToHeatGrid[_nodeGridPos.x, _nodeGridPos.y]){
		// 			if (_nodeGridPos == new Int2(10, 10)){
		// 				Debug.Log(_pth.HeatToGive + ", " + _pth.NodeGridPos);
		// 			}
		// 			particleGrid[_pth.NodeGridPos.x, _pth.NodeGridPos.y].Temperature += _pth.HeatToGive;
		// 			particleGrid[_nodeGridPos.x, _nodeGridPos.y].Temperature -= _pth.HeatToGive;
		// 		}
		// 		_particle.SetTemperature(_particle.Temperature);

		// 		particleGrid[_nodeGridPos.x, _nodeGridPos.y] = _particle;
		// 	}
		// }
	}

	void PopulateParticleToHeatGridAtPos(Int2 _nodeGridPos) {
		// List<ParticleToHeatData> _list = new List<ParticleToHeatData>();
		
		// if (GameGrid.IsInsideNodeGrid(_nodeGridPos + Int2.Up)){
		// 	_list.Add(new ParticleToHeatData());
		// }
		// if (GameGrid.IsInsideNodeGrid(_nodeGridPos + Int2.Down)){
		// 	_list.Add(new ParticleToHeatData());
		// }
		// if (GameGrid.IsInsideNodeGrid(_nodeGridPos + Int2.Left)){
		// 	_list.Add(new ParticleToHeatData());
		// }
		// if (GameGrid.IsInsideNodeGrid(_nodeGridPos + Int2.Right)){
		// 	_list.Add(new ParticleToHeatData());
		// }

		// particlesToHeatGrid[_nodeGridPos.x,_nodeGridPos.y] = _list;
	}

	void TrySetHeatTransferToParticle(Int2 _nodeGridPos, Int2 _nodeGridPosOther) {
		// if (!GameGrid.IsInsideNodeGrid(_nodeGridPosOther)){
		// 	return;
		// }

		// Particle _particle = particleGrid[_nodeGridPos.x, _nodeGridPos.y];
		// Particle _otherParticle = particleGrid[_nodeGridPosOther.x, _nodeGridPosOther.y];

		// Vector2 _dir = _otherParticle.Pos - _particle.Pos;
		// float _r2 = (_dir.x * _dir.x + _dir.y * _dir.y) * Mathf.Max(_particle.RepelFactor, _otherParticle.RepelFactor);

		// float _temperatureStartFrame = _particle.TemperatureStartFrame;
		// float _temperatureStartFrameOther = _otherParticle.TemperatureStartFrame;

		// if (_r2 > Particle.HSQ_TEMPERATURE){
		// 	return;
		// }
		// if (_temperatureStartFrame < _temperatureStartFrameOther){
		// 	return;
		// }
		// if (_temperatureStartFrameOther > Particle.MAX_TEMPERATURE){
		// 	return;
		// }

		// float _thermalDiffusivity = (_particle.GetThermalDiffusivity() + _otherParticle.GetThermalDiffusivity()) * 0.5f;
		// float _heatToGive = (_temperatureStartFrame - _temperatureStartFrameOther) * _thermalDiffusivity;

		// Vector4 heatToGive = _particle.HeatToGive;

		// List<ParticleToHeatData> particlesToHeat = particlesToHeatGrid[_nodeGridPos.x, _nodeGridPos.y];
		// particlesToHeat.Sort((x, y) => x.HeatToGive.CompareTo(y.HeatToGive));
		// ParticleToHeatData lowestPTH = particlesToHeat[0];

		// float _totalGive = 0.0f;
		// for (int i = 0; i < particlesToHeat.Count; i++){
		// 	_totalGive += particlesToHeat[i].HeatToGive;
		// }

		// _heatToGive = Mathf.Min(_heatToGive, _particle.TemperatureStartFrame - _totalGive);

		// if (_heatToGive > lowestPTH.HeatToGive){
		// 	lowestPTH.HeatToGive = _heatToGive;
		// 	lowestPTH.NodeGridPos = _nodeGridPosOther;
		// 	particlesToHeat[0] = lowestPTH;
		// }
	}

	void AddParticleToParticleGrid(Int2 _nodeGridPos, ElementSimulator.Particle _particle) {
		ElementSimulator.Particle _cachedParticle = particleGrid[_nodeGridPos.x, _nodeGridPos.y];
		if (_cachedParticle.Amount == 0.0f){
			_cachedParticle = _particle;
		}
		else{
			_cachedParticle.BlendVariablesWith(_particle);
		}

		particleGrid[_nodeGridPos.x, _nodeGridPos.y] = _cachedParticle;
	}

	void UpdateShader() {
		int binsThreadGroupCount = Mathf.CeilToInt((BIN_COUNT_X * BIN_COUNT_Y) / BINS_THREAD_COUNT);
		int particlesThreadGroupCountX = Mathf.CeilToInt(particles.Length / THREAD_COUNT_X);
		int outputThreadGroupCountX = Mathf.CeilToInt(GRID_WIDTH_PIXELS / OUTPUT_THREAD_COUNT_X);
		int outputThreadGroupCountY = Mathf.CeilToInt(GRID_HEIGHT_PIXELS / OUTPUT_THREAD_COUNT_Y);

		shader.SetBool(shaderPropertyID_isFirstFrame, isFirstFrame);
		shader.SetBool(shaderPropertyID_isEvenFrame, frame % 2 == 0);
		bufferParticles.SetData(particles);

		if (isFirstFrame){
			// Init
			shader.SetBuffer(kernelID_Init, shaderPropertyID_particles, bufferParticles);
			shader.SetInt(shaderPropertyID_particleCount, START_PARTICLE_COUNT_ACTIVE);
			shader.Dispatch(kernelID_Init, particlesThreadGroupCountX, 1, 1);

			// InitBins
			bufferBins.SetData(bins);
			bufferBinsAtStartFrame.SetData(binsAtStartFrame);
			shader.SetBuffer(kernelID_InitBins, shaderPropertyID_bins, bufferBins);
			shader.Dispatch(kernelID_InitBins, binsThreadGroupCount, 1, 1);
		}

		// ClearOutputTexture
		if (isFirstFrame){
			shader.SetTexture(kernelID_ClearOutputTexture, shaderPropertyID_output, output);
		}
		Profiler.BeginSample(KERNEL_CLEAROUTPUTTEXTURE);		
		shader.Dispatch(kernelID_ClearOutputTexture, outputThreadGroupCountX, outputThreadGroupCountY, 1);
		Profiler.EndSample();

		if (Time.time >= nextTimeToUpdateBins) { 
			nextTimeToUpdateBins = Time.time + updateIntervalBins;
		
			// CacheParticlesInBins
			if (isFirstFrame){
				shader.SetBuffer(kernelID_CacheParticlesInBins, shaderPropertyID_bins, bufferBins);
				shader.SetBuffer(kernelID_CacheParticlesInBins, shaderPropertyID_binsAtStartFrame, bufferBinsAtStartFrame);
				shader.SetBuffer(kernelID_CacheParticlesInBins, shaderPropertyID_particles, bufferParticles);
			}
			Profiler.BeginSample(KERNEL_CACHEPARTICLESINBINS);	
			shader.Dispatch(kernelID_CacheParticlesInBins, binsThreadGroupCount, 1, 1);
			Profiler.EndSample();

			// CacheParticleNeighbors
			if (isFirstFrame){
				shader.SetBuffer(kernelID_CacheClustersInBins, shaderPropertyID_bins, bufferBins);
			}
			Profiler.BeginSample(KERNEL_CACHECLUSTERSINBINS);	
			shader.Dispatch(kernelID_CacheClustersInBins, binsThreadGroupCount, 1, 1);
			Profiler.EndSample();
		}

		// ComputeDensity
		if (isFirstFrame){
			shader.SetBuffer(kernelID_ComputeDensity, shaderPropertyID_bins, bufferBins);
			shader.SetBuffer(kernelID_ComputeDensity, shaderPropertyID_particles, bufferParticles);
		}
		Profiler.BeginSample(KERNEL_COMPUTEDENSITY);	
		shader.Dispatch	(kernelID_ComputeDensity, particlesThreadGroupCountX, 1, 1);
		Profiler.EndSample();

		// ComputePressure
		if (isFirstFrame){
			shader.SetBuffer(kernelID_ComputePressure, shaderPropertyID_bins, bufferBins);
			shader.SetBuffer(kernelID_ComputePressure, shaderPropertyID_particles, bufferParticles);
		}
		Profiler.BeginSample(KERNEL_COMPUTEPRESSURE);	
		shader.Dispatch(kernelID_ComputePressure, particlesThreadGroupCountX, 1, 1);
		Profiler.EndSample();

		// ComputeForces
		if (isFirstFrame){
			shader.SetBuffer(kernelID_ComputeForces, shaderPropertyID_bins, bufferBins);
			shader.SetBuffer(kernelID_ComputeForces, shaderPropertyID_particles, bufferParticles);
		}
		Profiler.BeginSample(KERNEL_COMPUTEFORCES);	
		shader.Dispatch(kernelID_ComputeForces, particlesThreadGroupCountX, 1, 1);
		Profiler.EndSample();

		// Integrate
		if (isFirstFrame){
			shader.SetBuffer(kernelID_Integrate, shaderPropertyID_bins, bufferBins);
			shader.SetBuffer(kernelID_Integrate, shaderPropertyID_particles, bufferParticles);
			shader.SetTexture(kernelID_Integrate, shaderPropertyID_output, output);
		}
		Profiler.BeginSample(KERNEL_INTEGRATE);	
		shader.Dispatch(kernelID_Integrate, particlesThreadGroupCountX, 1, 1);
		Profiler.EndSample();

		bufferParticles.GetData(particles);
		material.mainTexture = output;

		frame++;
		isFirstFrame = false;
	}
}