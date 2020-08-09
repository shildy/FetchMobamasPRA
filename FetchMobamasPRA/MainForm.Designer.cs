namespace FetchMobamasPRA
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.textUrl = new System.Windows.Forms.TextBox();
            this.timerDocumentCompleted = new System.Windows.Forms.Timer(this.components);
            this.tableAddress = new System.Windows.Forms.TableLayoutPanel();
            this.labelAddress = new System.Windows.Forms.Label();
            this.tableMessage = new System.Windows.Forms.TableLayoutPanel();
            this.labelOutput = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableAddress.SuspendLayout();
            this.tableMessage.SuspendLayout();
            this.SuspendLayout();
            // 
            // textUrl
            // 
            this.textUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textUrl.Location = new System.Drawing.Point(68, 3);
            this.textUrl.Name = "textUrl";
            this.textUrl.ReadOnly = true;
            this.textUrl.Size = new System.Drawing.Size(713, 29);
            this.textUrl.TabIndex = 0;
            // 
            // timerDocumentCompleted
            // 
            this.timerDocumentCompleted.Interval = 1000;
            this.timerDocumentCompleted.Tick += new System.EventHandler(this.timerDocumentCompleted_Tick);
            // 
            // tableAddress
            // 
            this.tableAddress.ColumnCount = 2;
            this.tableAddress.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tableAddress.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableAddress.Controls.Add(this.labelAddress, 0, 0);
            this.tableAddress.Controls.Add(this.textUrl, 1, 0);
            this.tableAddress.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableAddress.Location = new System.Drawing.Point(0, 0);
            this.tableAddress.Name = "tableAddress";
            this.tableAddress.RowCount = 1;
            this.tableAddress.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableAddress.Size = new System.Drawing.Size(784, 35);
            this.tableAddress.TabIndex = 11;
            // 
            // labelAddress
            // 
            this.labelAddress.Location = new System.Drawing.Point(3, 0);
            this.labelAddress.Name = "labelAddress";
            this.labelAddress.Size = new System.Drawing.Size(59, 31);
            this.labelAddress.TabIndex = 0;
            this.labelAddress.Text = "アドレス:";
            this.labelAddress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tableMessage
            // 
            this.tableMessage.ColumnCount = 2;
            this.tableMessage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tableMessage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMessage.Controls.Add(this.labelOutput, 1, 0);
            this.tableMessage.Controls.Add(this.labelStatus, 0, 1);
            this.tableMessage.Controls.Add(this.label1, 0, 0);
            this.tableMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableMessage.Location = new System.Drawing.Point(0, 876);
            this.tableMessage.Name = "tableMessage";
            this.tableMessage.RowCount = 2;
            this.tableMessage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableMessage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableMessage.Size = new System.Drawing.Size(784, 65);
            this.tableMessage.TabIndex = 12;
            // 
            // labelOutput
            // 
            this.labelOutput.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelOutput.Location = new System.Drawing.Point(68, 0);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(713, 29);
            this.labelOutput.TabIndex = 3;
            this.labelOutput.Text = "Output";
            this.labelOutput.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelStatus
            // 
            this.labelStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableMessage.SetColumnSpan(this.labelStatus, 2);
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelStatus.Location = new System.Drawing.Point(3, 32);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(778, 29);
            this.labelStatus.TabIndex = 2;
            this.labelStatus.Text = "Status";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 31);
            this.label1.TabIndex = 1;
            this.label1.Text = "出力:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(784, 941);
            this.Controls.Add(this.tableMessage);
            this.Controls.Add(this.tableAddress);
            this.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "モバマス PRA取得";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.tableAddress.ResumeLayout(false);
            this.tableAddress.PerformLayout();
            this.tableMessage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timerDocumentCompleted;
        private System.Windows.Forms.TextBox textUrl;
        private System.Windows.Forms.TableLayoutPanel tableAddress;
        private System.Windows.Forms.Label labelAddress;
        private System.Windows.Forms.TableLayoutPanel tableMessage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelOutput;
        private System.Windows.Forms.Label labelStatus;
    }
}

