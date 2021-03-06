# UnityMMO
做游戏几年了,很多东西不好在工作项目上尝试(比如ECS),所以就有了本项目,我打算利用业余时间从头制作一个3D-MMO游戏,很多功能虽然都接触过,但我想换个做法(不然就不好玩了),反正没人逼着上线.前端上使用xlua,玩法系统用c#(主要是想用unity2018推的ECS,虽然目前还未完善各大系统都未对接不能用pure ecs),界面就用lua开发就行了.后端用Skynet(对比了其它几个开源项目后还是觉得skynet最简洁)  

# 使用方法
克隆本项目:git clone https://github.com/liuhaopen/UnityMMO.git --recurse  
要求:Unity2018以上并从菜单Window/Package Manager里下载Entities  
前端:  
下载下来后整个目录就是Unity的项目目录,用Unity打开,运行main.unity场景即可进入游戏的登录界面  
注:由于游戏资源过大且经常变更(每个版本的资源都会保存在.git文件夹里,clone就要好久了),所以放到另外的项目管理,可在[UnityMMO-Resource](https://github.com/liuhaopen/UnityMMO-Resource/tree/master/Assets/AssetBundleRes "UnityMMO-Resource")下载里面的文件并复制到本项目的Assets/AssetBundleRes里(注:有些插件因为版权问题就没上传了,从其中的download-page见购买链接)  
后端:  
)安装虚拟机,我使用的是CentOS7,然后设置整个项目目录为虚拟机的共享目录,cd到Server目录,先编译skynet:[skynet主页](https://github.com/cloudwu/skynet "skynet主页")  
)在虚拟机安装mysql并导入Server/data/里的两个数据库文件  
)运行:./run.sh跑起服务端  

# 各模块的技术选型
)玩法逻辑:使用Unity2018自带的ECS系统(要用Unity的ECS只能用C#)，服务端也用lua实现一套类似的ECS系统  
)界面逻辑:使用自制的基于组件的UI系统,全lua开发,动画也由lua实现了一份cocos的action  
)网络协议:使用sproto,玩法用c#版本,界面用lua版本  
)场景管理:用T2M切割地形为NxN小块,使用四叉树管理场景模型的动态加载  
)资源管理:使用Unity新版的AssetBundleBrowser打包资源，以逻辑类型分类（如角色，场景，怪物等），ab包增量更新  
)数据管理:使用redis,后面再看看要不要加入mysql  
)同步模式:基于请求回应的状态+差异同步  

# 已完成
)前端后端用skynet的loginserver通过登录验证  
)使用sproto协议,按模块分文件和id组,开发时拼接所有协议文件,发布版则预导出为二进制(支持lua和c#)  
)搭建apache服务器提供资源和lua代码的热更新  
)增加后端通用的mysql数据库服务  
)在PC和安卓手机平台测试通过了,可以连虚拟机上的服务端并登录  
)给xlua集成了lpeg,sproto,lua-crypt第三方库  
)创建玩家帐号数据库和相关操作服务  
)登录流程相关界面  
)导出场景信息（前端json后端lua格式）  
)九宫格加载场景块  
)玩家进入退出场景及坐标信息的同步  

# Todo
前端:   
)人物场景漫游-ECS做法(70%)  
)场景切割及动态加载(85%)  
)基于组件的UI框架(85%)  
)人物动作方面等Unity的新版Animation系统(IAnimationJob)完善后再介入吧  
)战斗系统  
)场景模型LOD,试试UnityGithub上的AutoLOD  

后端:  
)lua版本的ECS(70%)  
)人物的移动同步(80%)  
)NPC与怪物AI(5%)     
)使用Redis  
)AOI  

# 开发笔记
18.05.17：开始动工，以luaframework和skynet为基础快速搭建一个前后端骨架来。  
18.05.20：在linux搭建好skynet服务端后就开始着手协议模块了，对比了gpb和sproto最后还是选了后者，因为sproto简洁好多。协议按模块划分为不同的lua文件，每个模块占用最多100个id。开发模式时就在游戏启动时拼合所有模块的协议内容生成proto对象，正式发布时可以通过Tools/sprotodump工具预生成二进制文件提高初始化效率。  
18.05.24：skynet本身就提供了登录服务，所以后端不需要做多少东西。前端需要留意的就是读取网络字节流需要分两种方式，一种以大端方式仅取前两字节为包长度，一种是基于行区分网络包，详细见NetworkManager.cs，登录流程见LoginController.lua。  
18.05.30:skynet的登录验证用了lua-crypt库，但想在windows上使用还需要把几个linux仅有的接口改成windows版本的，还好要改的地方不多，然后加入tolua的run_time项目里用mingw生成新的tolua.dll（项目后来把tolua改成xlua了，做法也是差不多的）。  
18.06.02：用apache搭建了文件服务器，用于在手机跑游戏时热更代码和资源。遇到下载流程卡住不动，调试了下才知道是没处理好下载0kb文件的情况。  
18.06.11：luaframework的资源管理使用assetsbundle名和资源名加载，每次资源一改就要重新打包，这样项目一大就很蛋疼了，要打很久包。其实开发阶段就不需要打包，直接找本地资源就行了，但就需要建立ab名与具体目录的映射表，我在工作项目就是这么处理的，所以这里我换成引用资源时用相对于Assets目录的长路径，在正式发布包里把长路径转换为ab包名，发现和上个方案好不了多少。现在为了方便开发，许多资源都打在同一包里，后面再慢慢细分吧。  
18.06.20：简化游戏启动流程，因为启动状态有限，所以改成switch case的形式（之前的状态机算是过度设计了，各文件跳来跳去），然后统一放在Main.cs文件，状态间的跳转也在里面，只是每状态具体的工作交给各mgr处理，处理完后通过回调告知Main.cs然后在Main.cs里决定跳转去哪个状态，这样整个启动流程有哪些状态及跳转关系都一目了然了。  
18.07.11：初步可运行了，先上传到github上  
18.07.19：先做了个简单的协议分发器，收到前端发过来的协议时先解id号，然后根据其id号找到注册的模块及函数回调。  
18.07.23：做了一个简单的基于组件的ui框架，功能不再集中于一个ui manager，而是拆分为一个个组件挂在每个界面上，比如延迟销毁组件或隐藏底下所有界面的组件。先做了个登录界面，后面再慢慢补充组件完善功能。  
18.07.30：创建玩家帐号数据库，完成创建和选择角色进入游戏等协议。  
18.08.04：增加世界和场景服务，粗暴地广播全场景中各玩家的坐标变更信息，后面再加入aoi  
18.08.09：这几天心血来潮就把tolua换成xlua，主要是因为工作项目就是用tolua的，还没试过xlua，还好两者区别不大所以很快就改好了，用xlua的好处就是c#代码也可以热更，虽然很怀疑其性能，因为是运行时把某c#方法换为某lua函数，这个肯定不是迫不得已也不用的。  
10.08.19：写了几个UnityECS的System用于创建和操作角色，我打算和后端只用一条通用的同步协议，无论是有东西进入退出场景，或者是其某些状态变更了都通过那条通用同步协议告知，协议内容可搜索scene_get_objs_info_change。  
10.09.21：前后端可以同步各玩家坐标信息了，就是前端的模型太丑了而且还没有动作。  
10.09.26：增加场景分块加载模块，引用[场景分块加载方案](http://www.lsngo.net/2018/01/20/unity_quadtreescenemanage/?lydsvi=tnkcd2&wmzqhg=cl8dt "场景分块加载方案")，本来想用摄像机碰撞到地图块就显示的方式，但是在手机上性能一般，同时加载的地图块太多了，最后还是决定了把地图块分为高低模两级别，低模是进入大场景后预加载的，然后有一大一小九宫格，小九宫格用高模，边加载边释放不可见的，小九宫格就显示隐藏视角碰见的地图块低模。还有一种方案是低模不分块直接显示，高模用九宫格，但要处理复杂地型两高低模之间穿透问题，还有超大世界时整个低模也挺大的了，这个后面也要改成ECS做法的。  
18.10.02：因为要做大世界无缝加载，所以必须先分割大地图为n块，试了几个插件最后选择了Terrain To Mesh，然后写了个脚本导出各地图块及其上的细节物件的坐标等信息，生成为json格式。  
19.10.03：先加入第三人称控制插件，快速测试场景的分块加载，人物控制这块以后还要改成ECS实现方式的。  
19.10.09：增加摇杆界面，支持在手机上移动角色了  
18.10.10：考虑到资源越来越多，都放一起的话本项目clone会超级久，所以以后就把资源放在另外一个项目吧  
19.10.15：增加lua版本的cocos action  
19.10.25：增加本地存档接口cookiemgr，保存一些本地设置如历史输入帐号  
18.11.08：写脚本导出场景信息给后端使用（npc和怪物列表），直接生成lua代码。  
18.11.13：初步完成的大世界场景分块加载（还在考虑无限场景的NavMesh资源管理，先尝试用跳跃点连接n个地图块的navmesh）:   
![image](https://github.com/liuhaopen/ReadmeResources/blob/master/UnityMMO/run_in_terrain.gif)   
18.11.23：由于unity的entities版本更新修改较多导致之前的部分逻辑要重写了，感觉还是暂时先不弄前端的game play逻辑较好，先弄寻路和后端逻辑吧。开始写后端逻辑后想用ecs，考虑了两种方案：1把unity的entites去掉unity引擎的依赖并移植到linux然后导出接口给lua，2用lua实现一份ecs。最后选择了方案2，因为方案1需要在linux上运行c#，连带上了整个mono运行时，这就太累赘了。所以还是用lua实现多一份的好，虽然已经有很多第三方ecs库了，但还是自己实现的好玩。  
18.12.15：unity的entities框架从c#转换为lua实现时需要留意几点：1）数组从0起始，而lua习惯了从1开始，0索引会放在hash部分，所以大部分代码都要改成1起始，用起来也方便一点，有个例外就是ChunkDataUtility.lua里面的函数还是0起始，因为要直接操作指针。  
2）为了保证组件信息尽量存放在连续内存中，从而提高缓存命中率，组件信息都平坦地保存在名为Chunk的userdata里，引用时根据偏移算出指针地址进行读写。  
19.01.23：初步完成了可运行的lua版本ecs框架了（因为公司游戏上线忙死了所以基本上没什么时间弄），开始做后端的NPC和怪物逻辑吧，边做功能边给lua ecs补特性吧。后端的寻路就用recastnavigation库吧，要先导出接口给lua，其次是unity-navmesh导出的数据不能直接用的，所以还要写个脚本把unity生成的navmesh导出成recast支持的格式  
19.02.03：前端的寻路资源流程确定为大世界的navmesh分块打包（需要划分到不同的scene把所有节点删光，这样就只有navmesh数据了），然后运行时可以Additive模式加载该Scene，当然导出navmesh前还需要拉几个OffMeshLink，这样加载多个scene时会就自动把多个navmesh关联起来。  
19.02.13：终于把unity的navmesh导出成recastnavigation可以使用的数据了，主要思路是利用unity的NavMesh.CalculateTriangulation方法返回的三角形数据转化为recast里的rcPolyMesh对象然后再参照RecastDemo那样创建出dtNavMesh对象，详细见[Navigator](https://github.com/liuhaopen/Navigator "Navigator")  