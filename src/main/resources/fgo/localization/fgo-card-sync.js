const fs = require('fs-extra')
const path = require('path')
const ExcelJS = require('exceljs')
const readline = require('readline')

const JSON_FILE = './zhs/CardStrings.json'
const EN_JSON_FILE = './eng/CardStrings.json'
const CARDS_DIR = '../../../java/fgo/cards'
const OUTPUT_EXCEL_NORMAL = 'cards_normal.xlsx'
const OUTPUT_EXCEL_NOBLE = 'cards_noble.xlsx'

const MOD_ID_PREFIX = '${modID}:'

const excludeFiles = [
  'AbsNoblePhantasmCard.java',
  'FateMagineerCard.java',
  'FGOCard.java',
]

function parseJavaCard(code, filename) {
  const data = {
    cost: null,
    type: '',
    target: '',
    rarity: '',
    damage: null,
    block: null,
    magicNumber: null,
    np: null,
    star: null,
    damageUpgrade: null,
    blockUpgrade: null,
    magicUpgrade: null,
    npUpgrade: null,
    starUpgrade: null,
    costUpgrade: null,
    isNoble: false,
  }

  const superPattern =
    /super\s*\(\s*[^,]+\s*,\s*([+-]?\d+|\w+)\s*,\s*(?:AbstractCard\.)?CardType\.(\w+)\s*,\s*(?:AbstractCard\.)?CardTarget\.(\w+)\s*,\s*(?:AbstractCard\.)?CardRarity\.(\w+)(?:\s*,[^)]*)?\s*\)/
  const match = code.match(superPattern)

  if (match) {
    const [, costStr, typeStr, targetStr, rarityStr] = match
    data.type = typeStr
    data.target = targetStr
    data.rarity = rarityStr
    if (/^[+-]?\d+$/.test(costStr)) {
      data.cost = parseInt(costStr, 10)
    }
  } else {
    const altPattern =
      /super\s*\(\s*[^,]+\s*,\s*(?:AbstractCard\.)?CardType\.(\w+)\s*,\s*(?:AbstractCard\.)?CardTarget\.(\w+)\s*,\s*([+-]?\d+|\w+)\s*\)/
    const altMatch = code.match(altPattern)
    if (altMatch) {
      const [, typeStr, targetStr, costStr] = altMatch
      data.type = typeStr
      data.target = targetStr
      data.rarity = 'SPECIAL'
      if (/^[+-]?\d+$/.test(costStr)) {
        data.cost = parseInt(costStr, 10)
      }
      data.isNoble = true
    } else {
      console.log(`⚠️ 仍无法识别 super 构造器 in ${filename}.java`)
      const superLine = code
        .split('\n')
        .find(line => line.includes('super(') && !line.trim().startsWith('//'))
      if (superLine) console.log(`   → ${superLine.trim()}`)
    }
  }

  const dmgMatch = code.match(/setDamage\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)/)
  if (dmgMatch) {
    const base = parseInt(dmgMatch[1], 10)
    const delta = parseInt(dmgMatch[2], 10)
    data.damage = base
    data.damageUpgrade = base + delta
  }

  const blkMatch = code.match(/setBlock\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)/)
  if (blkMatch) {
    const base = parseInt(blkMatch[1], 10)
    const delta = parseInt(blkMatch[2], 10)
    data.block = base
    data.blockUpgrade = base + delta
  }

  const magMatch = code.match(/setMagic\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)/)
  if (magMatch) {
    const base = parseInt(magMatch[1], 10)
    const delta = parseInt(magMatch[2], 10)
    data.magicNumber = base
    data.magicUpgrade = base + delta
  }

  const npMatch = code.match(/setNP\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)/)
  if (npMatch) {
    const base = parseInt(npMatch[1], 10)
    const delta = parseInt(npMatch[2], 10)
    data.np = base
    data.npUpgrade = base + delta
  }

  const starMatch = code.match(/setStar\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)/)
  if (starMatch) {
    const base = parseInt(starMatch[1], 10)
    const delta = parseInt(starMatch[2], 10)
    data.star = base
    data.starUpgrade = base + delta
  }

  const costUpMatch = code.match(/setCostUpgrade\s*\(\s*(\d+)\s*\)/)
  if (costUpMatch) {
    data.costUpgrade = parseInt(costUpMatch[1], 10)
  }

  return data
}

function NL2LF(s) {
  if (!s) return ''
  return s.replace(/ NL /g, '\n')
}

function LF2NL(s) {
  if (!s) return ''
  return s.replace(/\n/g, ' NL ')
}

function scanJavaFiles(dir) {
  const javaFiles = []
  const items = fs.readdirSync(dir)
  for (const item of items) {
    const fullPath = path.join(dir, item)
    if (fs.statSync(fullPath).isDirectory()) {
      javaFiles.push(...scanJavaFiles(fullPath))
    } else if (item.endsWith('.java')) {
      const fileName = path.basename(fullPath)
      if (!excludeFiles.includes(fileName)) {
        javaFiles.push(fullPath)
      }
    }
  }
  return javaFiles
}

async function importExcelToJSON() {
  const allData = {}

  if (fs.existsSync(OUTPUT_EXCEL_NORMAL)) {
    const workbook = new ExcelJS.Workbook()
    await workbook.xlsx.readFile(OUTPUT_EXCEL_NORMAL)
    const worksheet = workbook.getWorksheet(1)

    let isHeader = true
    worksheet.eachRow((row, rowNumber) => {
      if (isHeader) {
        isHeader = false
        return
      }

      const idNonPrefix = row.getCell('A').value
      if (!idNonPrefix) return

      const id = MOD_ID_PREFIX + idNonPrefix

      const name = LF2NL(row.getCell('B').value || '')
      const desc = LF2NL(row.getCell('G').value || '')
      const upgradeDesc = LF2NL(row.getCell('P').value || '')
      const flavor = LF2NL(row.getCell('O').value || '')

      const entry = {
        NAME: name,
        DESCRIPTION: desc,
      }

      if (upgradeDesc) entry.UPGRADE_DESCRIPTION = upgradeDesc

      const extDescRaw = row.getCell('N').value || ''
      if (extDescRaw) {
        const descWithNewline = LF2NL(extDescRaw)
        entry.EXTENDED_DESCRIPTION = descWithNewline
          .split('\n')
          .filter(part => part !== '')
      }

      if (flavor) entry.FLAVOR = flavor
      allData[id] = entry
    })
    console.log(`✅ 导入卡片: ${Object.keys(allData).length} 条`)
  }

  if (fs.existsSync(OUTPUT_EXCEL_NOBLE)) {
    const workbook = new ExcelJS.Workbook()
    await workbook.xlsx.readFile(OUTPUT_EXCEL_NOBLE)
    const worksheet = workbook.getWorksheet(1)

    let isHeader = true
    worksheet.eachRow((row, rowNumber) => {
      if (isHeader) {
        isHeader = false
        return
      }

      const idNonPrefix = row.getCell('A').value
      if (!idNonPrefix) return

      const id = MOD_ID_PREFIX + idNonPrefix

      const name = LF2NL(row.getCell('B').value || '')
      const desc = LF2NL(row.getCell('G').value || '')
      const upgradeDesc = LF2NL(row.getCell('P').value || '')
      const flavor = LF2NL(row.getCell('O').value || '')

      const entry = {
        NAME: name,
        DESCRIPTION: desc,
      }

      if (upgradeDesc) entry.UPGRADE_DESCRIPTION = upgradeDesc

      const extDescRaw = row.getCell('N').value || ''
      if (extDescRaw) {
        const descWithNewline = LF2NL(extDescRaw)
        entry.EXTENDED_DESCRIPTION = descWithNewline
          .split('\n')
          .filter(part => part !== '')
      }

      if (flavor) entry.FLAVOR = flavor
      allData[id] = entry
    })
    console.log(`✅ 导入卡片: ${Object.keys(allData).length} 条 (宝具卡)`)
  }

  if (Object.keys(allData).length === 0) {
    console.log('❌ 未找到任何Excel文件，请先导出Excel')
    return
  }

  fs.writeFileSync(JSON_FILE, JSON.stringify(allData, null, 2))
  console.log(
    `✅ JSON文件已生成: ${JSON_FILE} (${Object.keys(allData).length} 个条目)`,
  )
}

async function importExcelToENJSON() {
  const enData = {}

  if (
    !fs.existsSync(OUTPUT_EXCEL_NORMAL) &&
    !fs.existsSync(OUTPUT_EXCEL_NOBLE)
  ) {
    console.log('❌ 未找到Excel文件，请先导出Excel')
    return
  }

  if (fs.existsSync(OUTPUT_EXCEL_NORMAL)) {
    const workbook = new ExcelJS.Workbook()
    await workbook.xlsx.readFile(OUTPUT_EXCEL_NORMAL)
    const worksheet = workbook.getWorksheet(1)

    let isHeader = true
    worksheet.eachRow((row, rowNumber) => {
      if (isHeader) {
        isHeader = false
        return
      }

      const idNonPrefix = row.getCell('A').value
      if (!idNonPrefix) return

      const id = MOD_ID_PREFIX + idNonPrefix

      const name = LF2NL(row.getCell('Q').value || '')
      const desc = LF2NL(row.getCell('R').value || '')
      const upgradeDesc = LF2NL(row.getCell('S').value || '')
      const flavor = LF2NL(row.getCell('U').value || '')

      const entry = {
        NAME: name,
        DESCRIPTION: desc,
      }

      if (upgradeDesc) entry.UPGRADE_DESCRIPTION = upgradeDesc

      const extDescRaw = row.getCell('T').value || ''
      if (extDescRaw) {
        const descWithNewline = LF2NL(extDescRaw)
        entry.EXTENDED_DESCRIPTION = descWithNewline
          .split('\n')
          .filter(part => part !== '')
      }

      if (flavor) entry.FLAVOR = flavor
      enData[id] = entry
    })
    console.log(`✅ 导入英文卡片: ${Object.keys(enData).length} 条`)
  }

  if (fs.existsSync(OUTPUT_EXCEL_NOBLE)) {
    const workbook = new ExcelJS.Workbook()
    await workbook.xlsx.readFile(OUTPUT_EXCEL_NOBLE)
    const worksheet = workbook.getWorksheet(1)

    let isHeader = true
    worksheet.eachRow((row, rowNumber) => {
      if (isHeader) {
        isHeader = false
        return
      }

      const idNonPrefix = row.getCell('A').value
      if (!idNonPrefix) return

      const id = MOD_ID_PREFIX + idNonPrefix

      const name = LF2NL(row.getCell('Q').value || '')
      const desc = LF2NL(row.getCell('R').value || '')
      const upgradeDesc = LF2NL(row.getCell('S').value || '')
      const flavor = LF2NL(row.getCell('U').value || '')

      const entry = {
        NAME: name,
        DESCRIPTION: desc,
      }

      if (upgradeDesc) entry.UPGRADE_DESCRIPTION = upgradeDesc

      const extDescRaw = row.getCell('T').value || ''
      if (extDescRaw) {
        const descWithNewline = LF2NL(extDescRaw)
        entry.EXTENDED_DESCRIPTION = descWithNewline
          .split('\n')
          .filter(part => part !== '')
      }

      if (flavor) entry.FLAVOR = flavor
      enData[id] = entry
    })
    console.log(`✅ 导入英文卡片: ${Object.keys(enData).length} 条 (宝具卡)`)
  }

  if (Object.keys(enData).length === 0) {
    console.log('❌ 未找到任何Excel文件，请先导出Excel')
    return
  }

  fs.writeFileSync(EN_JSON_FILE, JSON.stringify(enData, null, 2))
  console.log(
    `✅ 英文JSON文件已生成: ${EN_JSON_FILE} (${Object.keys(enData).length} 个条目)`,
  )
}

async function exportJSONToExcel() {
  const zhJsonData = JSON.parse(fs.readFileSync(JSON_FILE, 'utf-8'))
  let enJsonData = {}
  try {
    enJsonData = JSON.parse(fs.readFileSync(EN_JSON_FILE, 'utf-8'))
  } catch (e) {
    console.warn(`⚠️ 未找到英文 JSON 文件: ${EN_JSON_FILE}，英文字段将留空`)
  }
  const normalRows = []
  const nobleRows = []

  const enHeader = [
    'Name',
    'Description',
    'Description+',
    'Extended Description',
    'Flavor',
  ]

  const normalHeader = [
    'ID',
    '牌名',
    '稀有度',
    '类型',
    '目标',
    '费用',
    '描述',
    '伤害值',
    '格挡值',
    '特殊值',
    '宝具值',
    '暴击星',
    '扩展描述',
    '风味文字',
    '费用+',
    '描述+',
    '伤害值+',
    '格挡值+',
    '特殊值+',
    '宝具值+',
    '暴击星+',
    '泛用性',
    '功能',
    ...enHeader,
  ]

  const nobleHeader = [
    'ID',
    '牌名',
    '类型',
    '目标',
    '费用',
    '描述',
    '伤害值',
    '格挡值',
    '特殊值',
    '宝具值',
    '暴击星',
    '扩展描述',
    '风味文字',
    '费用+',
    '描述+',
    '伤害值+',
    '格挡值+',
    '特殊值+',
    '宝具值+',
    '暴击星+',
    '泛用性',
    '功能',
    ...enHeader,
  ]

  const javaCardInfo = {}
  const javaFiles = scanJavaFiles(CARDS_DIR)
  for (const javaPath of javaFiles) {
    const cardId = path.basename(javaPath, '.java')
    try {
      const code = fs.readFileSync(javaPath, 'utf-8')
      javaCardInfo[cardId] = parseJavaCard(code, cardId)
    } catch (e) {
      console.error(`❌ 读取失败 ${javaPath}:`, e.message)
    }
  }

  for (const [fullKey, zhInfo] of Object.entries(zhJsonData)) {
    let cardId
    if (fullKey.startsWith(MOD_ID_PREFIX)) {
      cardId = fullKey.slice(MOD_ID_PREFIX.length)
    } else {
      cardId = fullKey
    }

    const javaInfo = javaCardInfo[cardId] || {
      cost: null,
      type: '',
      target: '',
      rarity: '',
      damage: null,
      block: null,
      magicNumber: null,
      np: null,
      star: null,
      damageUpgrade: null,
      blockUpgrade: null,
      magicUpgrade: null,
      npUpgrade: null,
      starUpgrade: null,
      costUpgrade: null,
      isNoble: false,
    }

    const enInfo = enJsonData[fullKey] || enJsonData[cardId] || {}

    const enFields = {
      Name: NL2LF(enInfo.NAME || ''),
      Description: NL2LF(enInfo.DESCRIPTION || ''),
      'Description+': NL2LF(enInfo.UPGRADE_DESCRIPTION || ''),
      'Extended Description': enInfo.EXTENDED_DESCRIPTION
        ? enInfo.EXTENDED_DESCRIPTION.map(part => NL2LF(part)).join('\n')
        : '',
      Flavor: NL2LF(enInfo.FLAVOR || ''),
    }

    const commonFields = {
      ID: cardId,
      牌名: zhInfo.NAME || '',
      类型: javaInfo.type,
      目标: javaInfo.target,
      费用: javaInfo.cost,
      描述: NL2LF(zhInfo.DESCRIPTION || ''),
      伤害值: javaInfo.damage,
      格挡值: javaInfo.block,
      特殊值: javaInfo.magicNumber,
      宝具值: javaInfo.np,
      暴击星: javaInfo.star,
      扩展描述: zhInfo.EXTENDED_DESCRIPTION
        ? zhInfo.EXTENDED_DESCRIPTION.map(part => NL2LF(part)).join('\n')
        : '',
      风味文字: NL2LF(zhInfo.FLAVOR || ''),
      '费用+': javaInfo.costUpgrade,
      '描述+': NL2LF(zhInfo.UPGRADE_DESCRIPTION || ''),
      '伤害值+': javaInfo.damageUpgrade,
      '格挡值+': javaInfo.blockUpgrade,
      '特殊值+': javaInfo.magicUpgrade,
      '宝具值+': javaInfo.npUpgrade,
      '暴击星+': javaInfo.starUpgrade,
      泛用性: '',
      功能: '',
      ...enFields,
    }

    if (javaInfo.isNoble) {
      nobleRows.push(commonFields)
    } else {
      normalRows.push({
        ...commonFields,
        稀有度: javaInfo.rarity,
      })
    }
  }

  if (normalRows.length > 0) {
    const workbook = new ExcelJS.Workbook()
    const worksheet = workbook.addWorksheet('Cards')

    // 设置表头
    worksheet.columns = normalHeader.map((header, index) => ({
      header: header,
      key: header,
      width: 20,
    }))

    // 添加数据行
    normalRows.forEach(row => {
      worksheet.addRow(row)
    })

    await workbook.xlsx.writeFile(OUTPUT_EXCEL_NORMAL)
    console.log(
      `✅ 普通卡 Excel 已生成: ${OUTPUT_EXCEL_NORMAL} (${normalRows.length} 张卡)`,
    )
  }

  if (nobleRows.length > 0) {
    const workbook = new ExcelJS.Workbook()
    const worksheet = workbook.addWorksheet('Noble Cards')

    // 设置表头
    worksheet.columns = nobleHeader.map((header, index) => ({
      header: header,
      key: header,
      width: 20,
    }))

    // 添加数据行
    nobleRows.forEach(row => {
      worksheet.addRow(row)
    })

    await workbook.xlsx.writeFile(OUTPUT_EXCEL_NOBLE)
    console.log(
      `✅ 宝具卡 Excel 已生成: ${OUTPUT_EXCEL_NOBLE} (${nobleRows.length} 张卡)`,
    )
  }
}

async function prompt(question) {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
  })

  return new Promise(resolve => {
    rl.question(question, answer => {
      rl.close()
      resolve(answer)
    })
  })
}

async function main() {
  console.log(`\n===== 卡片数据管理工具 (v1.0) =====`)
  console.log(`1. 导出Excel (从Java代码生成)`)
  console.log(`2. 导入JSON (从Excel生成CardStrings.json)`)
  console.log(`================================\n`)

  if (process.argv.includes('--export')) {
    console.log('\n▶ 正在导出Excel...')
    await exportJSONToExcel()
    return
  }
  if (process.argv.includes('--import')) {
    console.log('\n▶ 正在导入JSON...')
    await importExcelToJSON()
    await importExcelToENJSON()
    return
  }

  const choice = await prompt('请选择操作 (输入1或2): ')

  switch (choice.trim()) {
    case '1':
      console.log('\n▶ 正在导出Excel...')
      await exportJSONToExcel()
      break
    case '2':
      console.log('\n▶ 正在导入JSON...')
      await importExcelToJSON()
      await importExcelToENJSON()
      break
    default:
      console.log('❌ 无效选择！请使用 1 或 2')
  }
}

main().catch(console.error)
