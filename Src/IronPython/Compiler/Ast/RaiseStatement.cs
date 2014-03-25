/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;

#if FEATURE_CORE_DLR
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    public class RaiseStatement : Statement {
        private readonly Expression _type, _cause;
        private bool _inFinally;

        public RaiseStatement(Expression exception, Expression cause) {
            _type = exception;
            _cause = cause;
        }

        [Obsolete("Type is obsolete due to direct inheritance from DLR Expression.  Use ExceptType instead")]
        public new Expression Type {
            get { return _type; }
        }

        public Expression ExceptType {
            get {
                return _type;
            }
        }

        public Expression Cause {
            get { return _cause; }
        }

        public override MSAst.Expression Reduce() {
            MSAst.Expression raiseExpression;
            if (_type == null) {
                raiseExpression = Ast.Call(
                    AstMethods.MakeRethrownException,
                    Parent.LocalContext
                );
                
                if (!InFinally) {
                    raiseExpression = Ast.Block(
                        UpdateLineUpdated(true),
                        raiseExpression
                    );
                }
            } else {
                raiseExpression = Ast.Call(
                    AstMethods.MakeException,
                    Parent.LocalContext,
                    TransformOrConstantNull(_type, typeof(object)),
                    // TODO: python3
                    TransformOrConstantNull(null, typeof(object)),
                    TransformOrConstantNull(null, typeof(object))
                );
            }

            return GlobalParent.AddDebugInfo(
                Ast.Throw(raiseExpression),
                Span
            );
        }

        internal bool InFinally {
            get {
                return _inFinally;
            }
            set {
                _inFinally = value;
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_type != null) {
                    _type.Walk(walker);
                }
                if (_cause != null) {
                    _cause.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
