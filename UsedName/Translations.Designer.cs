﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace UsedName {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Translations {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Translations() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("UsedName.Translations", typeof(Translations).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 {
        ///    &quot;Update FriendList&quot;: &quot;更新好友列表&quot;,
        ///    &quot;Language:&quot;: &quot;语言:&quot;,
        ///    &quot;Name Change Check&quot;: &quot;检查姓名变更&quot;,
        ///    &quot;Enable Search In Context&quot;: &quot;启用右键菜单搜索&quot;,
        ///    &quot;Search in Context String&quot;: &quot;搜索按钮&quot;,
        ///    &quot;Enable Add Nick Name&quot;: &quot;启用右键添加昵称&quot;,
        ///    &quot;Add Nick Name String&quot;: &quot;添加昵称按钮&quot;,
        ///    &quot;Show player who changed name when update FriendList&quot;: &quot;更新好友列表时，显示姓名变更&quot;,
        ///    &quot;Update FriendList completed&quot;: &quot;更新好友列表完成&quot;,
        ///    &quot;Parameter error, length is &apos;{0}&apos;&quot;: &quot;参数错误，长度为&apos;{0}&apos;&quot;,
        ///    &quot;Invalid parameter: &quot;: &quot;非法参数: &quot;,
        ///    &quot; changed name to &quot;: [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        public static string zh_CN {
            get {
                return ResourceManager.GetString("zh_CN", resourceCulture);
            }
        }
    }
}
